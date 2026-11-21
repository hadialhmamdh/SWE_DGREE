package model.tiles.units.enemies;

import model.tiles.units.HeroicUnit;
import model.tiles.units.players.Player;
import utils.Position;

public class Boss extends Monster implements HeroicUnit {
    private int abilityFrequency;
    private int combatTicks;

    public Boss(char enemySymbol, String name, int hitpoints, int attack, int defense,
                int experienceValue, int visionRange, int abilityFrequency) {
        super(enemySymbol, name, hitpoints, attack, defense, experienceValue, visionRange);
        this.abilityFrequency = abilityFrequency;
        this.combatTicks = 0;
    }

    @Override
    public void onTurn() {
        Position playerPos = observer.getPlayerP().getPosition();
        Position myPos = getPosition();
        if (myPos.range(playerPos) < visionRange) {
            if (combatTicks == abilityFrequency) {
                combatTicks = 0;
                castAbility();
            } else {
                combatTicks++;
                int dx = myPos.getX() - playerPos.getX();
                int dy = myPos.getY() - playerPos.getY();
                int x = myPos.getX();
                int y = myPos.getY();
                if (Math.abs(dx) > Math.abs(dy)) {
                    if (dx > 0)
                        interact(observer.getTileP(new Position(x - 1, y))); // שמאלה
                    else
                        interact(observer.getTileP(new Position(x + 1, y))); // ימינה
                } else {
                    if (dy > 0)
                        interact(observer.getTileP(new Position(x, y - 1))); // למעלה
                    else
                        interact(observer.getTileP(new Position(x, y + 1))); // למטה
                }
            }
        } else {
            combatTicks = 0;
            randomAction();
        }
    }

    @Override
    public void castAbility() {
        Player player = observer.getPlayerP();
        int defense = player.defend();
        messageCallback.send(player.getName() + " rolled " + defense + " defense points");
        int damage = Math.max(0, attack - defense);
        messageCallback.send(getName() + " shoots " + player.getName() + " for " + damage + " ability damage");
        player.getHealth().setCurrent(player.getHealth().getCurrent() - damage);
        if (!player.alive()) {
            player.onDeath();
        }
    }
    @Override
    public boolean visiable() {
        return true;
    }
    public String description() {
        return super.description()+ " Ability frequency: "+this.abilityFrequency;
    }

}
