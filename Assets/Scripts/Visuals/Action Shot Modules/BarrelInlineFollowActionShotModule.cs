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
    }
    /// <summary>
    /// Refreshes the action shot.
    /// </summary>
    public override void Refresh()
    {
        base.Refresh();
        Projectile projectile = selectedTurret.recentlyFiredProjectiles[trackedProjectileID];
        if (projectile != null)
        {
            fired = true;
            Vector3 direction = projectile.velocity.normalized;
            float xzDistance = Vector2.Distance(Vector2.zero, new Vector2(direction.x, direction.z));
            Vector3 angle = new Vector3(Mathf.Atan2(-direction.y, xzDistance) * Mathf.Rad2Deg, Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, 0f);
            //angle.x = -30f;
            //angle.y += 90f;
            Cameraman.TakePosition(new Cameraman.CameraPosition(0.22f, projectile.transform.position + Vector3.up, angle));
        }
        else
        {
            if (fired && (stage == 1 || stage == 2))
            {
                stage = 3;
                //Vector3 position = BattleInterface.battle.defendingPlayer.board.tiles[(int)BattleInterface.battle.recentlyShot.x, (int)BattleInterface.battle.recentlyShot.y].worldPosition;
                Vector3 position = BattleInterface.battle.recentAttackInfo.attackedTileWorldPosition;
                position.y = 8f;
                Cameraman.TakePosition(new Cameraman.CameraPosition(0.5f, position, new Vector3(90f, Camera.main.transform.eulerAngles.y, 0f)));
            }
        }

        switch (stage)
        {
            case 0:
                if (lifetime > 1f)
                {
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
                        stage = 2;
                        selectedTurret.Fire(1f);
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
                    stage = 1;
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
