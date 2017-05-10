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
    /// Prepares the action shot.
    /// </summary>
    public override void Prepare()
    {
        base.Prepare();

        List<Turret> availableTurrets = new List<Turret>();

        if (BattleInterface.battle.recentAttackInfo.hitShips.Count > 0)
        {
            if (BattleInterface.battle.recentAttackInfo.hitShips[0].eliminated)
            {
                //killingShot = true;
            }

            if ((GameController.humanPlayers == 1 && !BattleInterface.battle.defendingPlayer.AI) || GameController.humanPlayers == 0 || killingShot)
            {
                BattleInterface.battle.recentAttackInfo.hitShips[0].gameObject.SetActive(true);
            }
        }

        foreach (Ship ship in attackers)
        {
            //ship.PrepareToFireAt(BattleInterface.battle.defendingPlayer.board.tiles[(int)BattleInterface.battle.recentlyShot.x, (int)BattleInterface.battle.recentlyShot.y].worldPosition, BattleInterface.battle.defendingPlayer.board.tiles[(int)BattleInterface.battle.recentlyShot.x, (int)BattleInterface.battle.recentlyShot.y].containedShip);
            ship.PrepareToFireAt(BattleInterface.battle.recentAttackInfo.hitTiles[0].transform.position, BattleInterface.battle.recentAttackInfo.hitShips.Count > 0 ? BattleInterface.battle.recentAttackInfo.hitShips[0] : null);
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
            if (selectedShip != null && !killingShot)
            {
                Vector3 direction = selectedTurret.gunDirection;
                float xzDistance = Vector2.Distance(Vector2.zero, new Vector2(direction.x, direction.z));
                Vector3 angle = new Vector3(Mathf.Atan2(-direction.y, xzDistance) * Mathf.Rad2Deg, Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, 0f);

                direction.y = 0f;
                Cameraman.TakePosition(new Cameraman.CameraPosition(0.25f, selectedTurret.transform.position + Vector3.up * 0.3f - direction, angle));
            }
        }





        if (attackers.Count == 0)
        {
            Actionman.EndActionView();
        }
    }

    /// <summary>
    /// Final stage camera offset.
    /// </summary>
    Vector3 finalStageCameraOffset = Vector3.zero;
    /// <summary>
    /// Whether to follow the projectile.
    /// </summary>
    bool followProjectile = false;
    /// <summary>
    /// Whether the focused turret has fired.
    /// </summary>
    bool fired = false;
    /// <summary>
    /// Whether the shot will kill the targeted ship.
    /// </summary>
    bool killingShot = false;
    /// <summary>
    /// The actual followed projectile.
    /// </summary>
    Projectile projectile;
    /// <summary>
    /// Refreshes the action shot.
    /// </summary>
    public override void Refresh()
    {
        base.Refresh();
        //killingShot = true;
        if (killingShot)
        {
            switch (stage)
            {
                case 0:
                    CalculateEndStageCameraOffsetDirection();
                    EndStageCamera();
                    stage = 1;
                    break;
                case 1:
                    if (Cameraman.transitionProgress > 98f)
                    {
                        FireAllShipGuns();
                        stage = 2;
                    }
                    break;
                case 2:
                    if (lifetime > 3.5f)
                    {
                        Actionman.EndActionView();
                    }
                    break;
            }
        }
        else
        {
            switch (stage)
            {
                case 0:
                    if (Cameraman.transitionProgress > 98.75f)
                    {
                        FireAllShipGuns();
                        CalculateEndStageCameraOffsetDirection();
                        stage = 1;
                    }
                    break;
                case 1:
                    if (FollowProjectile())
                    {
                        stage = 2;
                    }
                    break;
                case 2:
                    if (FollowProjectile())
                    {
                        if (projectile.travelTimeLeft / projectile.TravelTime < 0.2f)
                        {
                            EndStageCamera();
                            stage = 3;
                        }
                    }
                    else
                    {
                        EndStageCamera();
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
        }
    }

    void CalculateEndStageCameraOffsetDirection()
    {
        finalStageCameraOffset = -(BattleInterface.battle.recentAttackInfo.hitTiles[0].transform.position - selectedTurret.transform.position).normalized;
    }

    void EndStageCamera()
    {
        followProjectile = false;
        if (killingShot)
        {
            Vector3 angle = new Vector3(40f, Mathf.Atan2(finalStageCameraOffset.x, finalStageCameraOffset.z) * Mathf.Rad2Deg, 0f);
            float elevation = 15f;
            float transitionTime = 0.6f;
            float horizontalDistance = Mathf.Tan((90f - angle.x) * Mathf.Deg2Rad) * elevation;
            Vector3 position = BattleInterface.battle.recentAttackInfo.hitTiles[0].transform.position - finalStageCameraOffset * horizontalDistance;
            position.y = elevation;
            Cameraman.TakePosition(new Cameraman.CameraPosition(transitionTime, position, angle));
        }
        else
        {
            Vector3 angle = new Vector3(45f, Camera.main.transform.eulerAngles.y, 0f);
            float elevation = 8f;
            float transitionTime = 0.5f;
            float horizontalDistance = Mathf.Tan((90f - angle.x) * Mathf.Deg2Rad) * elevation;
            Vector3 position = BattleInterface.battle.recentAttackInfo.hitTiles[0].transform.position + finalStageCameraOffset * horizontalDistance;
            position.y = elevation;
            Cameraman.TakePosition(new Cameraman.CameraPosition(transitionTime, position, angle));
        }
    }

    bool FollowProjectile()
    {
        projectile = selectedTurret.recentlyFiredProjectiles[trackedProjectileID];
        if (projectile != null)
        {
            Vector3 direction = projectile.velocity.normalized;
            float xzDistance = Vector2.Distance(Vector2.zero, new Vector2(direction.x, direction.z));
            Vector3 angle = new Vector3(Mathf.Atan2(-direction.y, xzDistance) * Mathf.Rad2Deg, Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, 0f);
            Cameraman.TakePosition(new Cameraman.CameraPosition(0.22f, projectile.transform.position + Vector3.up, angle));
        }
        return projectile != null;
    }

    void FireAllShipGuns()
    {
        foreach (Ship ship in attackers)
        {
            ship.Fire();
        }
    }

}
