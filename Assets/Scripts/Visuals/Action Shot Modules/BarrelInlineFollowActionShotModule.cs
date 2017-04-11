using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelInlineFollowActionShotModule : FleetAttackFormationBaseModule
{
    /// <summary>
    /// The ID of the gun which fired the projectile tracked by the action camera.
    /// </summary>
    int trackedProjectileID;
    /// <summary>
    /// The turret the action camera will focus on.
    /// </summary>
    Turret selectedTurret;
    /// <summary>
    /// The ship the action camera will focus on.
    /// </summary>
    Ship selectedShip;
    /// <summary>
    /// Whether the focused turret has fired.
    /// </summary>
    bool fired = false;
    /// <summary>
    /// Whether the shot will kill the targeted ship.
    /// </summary>
    bool killingShot = false;
    /// <summary>
    /// Prepares the action shot.
    /// </summary>
    public override void Prepare()
    {
        base.Prepare();

        List<Turret> availableTurrets = new List<Turret>();

        foreach (Ship ship in attackers)
        {
            //ship.PrepareToFireAt(BattleInterface.battle.defendingPlayer.board.tiles[(int)BattleInterface.battle.recentlyShot.x, (int)BattleInterface.battle.recentlyShot.y].worldPosition, BattleInterface.battle.defendingPlayer.board.tiles[(int)BattleInterface.battle.recentlyShot.x, (int)BattleInterface.battle.recentlyShot.y].containedShip);
            ship.PrepareToFireAt(BattleInterface.battle.recentAttackInfo.attackedTileWorldPosition, BattleInterface.battle.recentAttackInfo.hitShip);
            foreach (Turret turret in ship.turrets)
            {
                if (turret.canFire && !turret.ignoredByActionCamera)
                {
                    availableTurrets.Add(turret);
                }
            }

        }

        if (availableTurrets.Count > 0)
        {
            selectedTurret = availableTurrets[Random.Range(0, availableTurrets.Count)];
            selectedShip = selectedTurret.ship;
            if (selectedShip != null)
            {
                Vector3 direction = selectedTurret.gunDirection;
                float xzDistance = Vector2.Distance(Vector2.zero, new Vector2(direction.x, direction.z));
                Vector3 angle = new Vector3(Mathf.Atan2(-direction.y, xzDistance) * Mathf.Rad2Deg, Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, 0f);

                direction.y = 0f;
                Cameraman.TakePosition(new Cameraman.CameraPosition(0.25f, selectedTurret.transform.position + Vector3.up * 0.3f - direction, angle));
            }
        }

        if (BattleInterface.battle.recentAttackInfo.hitShip != null)
        {
            if (BattleInterface.battle.recentAttackInfo.hitShip.eliminated)
            {
                killingShot = true;
                BattleInterface.battle.recentAttackInfo.hitShip.gameObject.SetActive(true);
            }
        }

        if (attackers.Count == 0)
        {
            Actionman.EndActionView();
        }
    }
    Vector3 finalStageCameraOffset = Vector3.zero;

    /// <summary>
    /// Refreshes the action shot.
    /// </summary>
    public override void Refresh()
    {
        base.Refresh();
        //killingShot = true;
        Projectile projectile = selectedTurret.recentlyFiredProjectiles[trackedProjectileID];
        bool switchToFinalStage = false;
        if (projectile != null)
        {
            if (stage == 1 || stage == 2)
            {
                if (!killingShot)
                {
                    fired = true;
                    Vector3 direction = projectile.velocity.normalized;
                    float xzDistance = Vector2.Distance(Vector2.zero, new Vector2(direction.x, direction.z));
                    Vector3 angle = new Vector3(Mathf.Atan2(-direction.y, xzDistance) * Mathf.Rad2Deg, Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, 0f);
                    //angle.x = -30f;
                    //angle.y += 90f;
                    Cameraman.TakePosition(new Cameraman.CameraPosition(0.22f, projectile.transform.position + Vector3.up, angle));
                    switchToFinalStage = projectile.travelTimeLeft / projectile.TravelTime < 0.2f;
                }
            }
        }
        else
        {
            switchToFinalStage = fired && (stage == 1 || stage == 2);
        }

        if (switchToFinalStage && !killingShot)
        {
            stage = 3;
            //killingShot = true;
            //Vector3 position = BattleInterface.battle.defendingPlayer.board.tiles[(int)BattleInterface.battle.recentlyShot.x, (int)BattleInterface.battle.recentlyShot.y].worldPosition;
            Vector3 angle = new Vector3(45f, Camera.main.transform.eulerAngles.y, 0f);
            float elevation = 8f;
            float transitionTime = 0.5f;
            //float horizontalDistance = 1f;
            float horizontalDistance = Mathf.Tan((90f - angle.x) * Mathf.Deg2Rad) * elevation;
            Vector3 position = BattleInterface.battle.recentAttackInfo.attackedTileWorldPosition + finalStageCameraOffset * horizontalDistance;
            position.y = elevation;
            Cameraman.TakePosition(new Cameraman.CameraPosition(transitionTime, position, angle));
            //switchToFinalStage = false;
        }

        switch (stage)
        {
            case 0:
                if (lifetime > 1f)
                {
                    finalStageCameraOffset = -(BattleInterface.battle.recentAttackInfo.attackedTileWorldPosition - selectedTurret.transform.position).normalized;
                    finalStageCameraOffset.y = 0f;

                    if (!killingShot)
                    {
                        stage = 1;
                        foreach (Ship ship in attackers)
                        {
                            ship.Fire();
                        }
                    }
                    else
                    {
                        Vector3 angle = new Vector3(40f, Camera.main.transform.eulerAngles.y + 180f, 0f);
                        float elevation = 15f;
                        float transitionTime = 0.6f;
                        //finalStageCameraOffset *= -1;
                        float horizontalDistance = Mathf.Tan((90f - angle.x) * Mathf.Deg2Rad) * elevation;
                        Vector3 position = BattleInterface.battle.recentAttackInfo.attackedTileWorldPosition - finalStageCameraOffset * horizontalDistance;
                        position.y = elevation;
                        Cameraman.TakePosition(new Cameraman.CameraPosition(transitionTime, position, angle));

                        stage = 2;
                        selectedTurret.Fire(1f);
                        fired = true;
                    }


                }
                break;
            case 2:
                if (lifetime > 2.2f && fired)
                {
                    foreach (Ship ship in attackers)
                    {
                        if (ship != selectedShip)
                        {
                            ship.Fire();
                        }
                    }
                    stage = 3;
                }
                break;
            case 3:
                if (Cameraman.transitionProgress > 98f)
                {
                    Actionman.EndActionView();
                }
                break;
        }




        // if (stage == 3 && Cameraman.transitionProgress > 98f && (killingShot && lifetime > 10f || !killingShot))
        // {
        //     Actionman.EndActionView();
        // }
    }
}
