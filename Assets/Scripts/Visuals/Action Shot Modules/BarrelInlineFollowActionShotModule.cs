using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelInlineFollowActionShotModule : FleetAttackFormationBaseModule
{
    int trackedProjectileID;
    Turret selectedTurret;
    Ship selectedShip;
    bool fired = false;
    bool killingShot = false;
    public override void Prepare()
    {
        base.Prepare();

        List<Turret> availableTurrets = new List<Turret>();

        foreach (Ship ship in attackers)
        {
            ship.PrepareToFireAt(BattleInterface.battle.defendingPlayer.board.tiles[(int)BattleInterface.battle.recentlyShot.x, (int)BattleInterface.battle.recentlyShot.y].worldPosition, BattleInterface.battle.defendingPlayer.board.tiles[(int)BattleInterface.battle.recentlyShot.x, (int)BattleInterface.battle.recentlyShot.y].containedShip);
            foreach (Turret turret in ship.turrets)
            {
                if (turret.canFire)
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

        if (selectedShip.targetedShip != null)
        {
            if (selectedShip.targetedShip.eliminated)
            {
                killingShot = true;
                selectedShip.targetedShip.gameObject.SetActive(true);
            }
        }
    }

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
            if (fired && stage == 1)
            {
                stage = 2;
                Vector3 position = BattleInterface.battle.defendingPlayer.board.tiles[(int)BattleInterface.battle.recentlyShot.x, (int)BattleInterface.battle.recentlyShot.y].worldPosition;
                position.y = 8f;
                Cameraman.TakePosition(new Cameraman.CameraPosition(0.5f, position, new Vector3(90f, Camera.main.transform.eulerAngles.y, 0f)));

            }
        }


        if (lifetime > 1f && stage == 0)
        {
            stage = 1;
            if (!killingShot)
            {
                foreach (Ship ship in BattleInterface.battle.attackingPlayer.livingShips)
                {
                    if (ship.length == ship.lengthRemaining)
                    {
                        ship.Fire();
                    }
                }
            }
            else
            {
                selectedShip.Fire();
            }
        }

        if (stage == 2 && Cameraman.transitionProgress > 98f && (killingShot && lifetime > 10f || !killingShot))
        {
            Actionman.EndActionView();
        }
    }
}
