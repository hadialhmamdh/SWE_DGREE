package model.tiles.units.enemies;

import model.tiles.units.players.Player;
import utils.Health;

public class Trap extends Enemy{
    private int visibilityTime;
    private int invisibilityTime;
    private int ticksCount;
    private boolean visible;

    public Trap(char enemySympol, String name, int hitpoints, int attack, int defense, int experienceValue, int visibilityTime, int invisibilityTime){
        super(enemySympol,name,hitpoints, attack, defense,experienceValue);
        this.visibilityTime=visibilityTime;
        this.invisibilityTime=invisibilityTime;
        this.ticksCount=0;
        this.visible=true;
    }

    @Override
    public void onTurn() {
        visible = ticksCount < visibilityTime;
        ticksCount++;
        if (ticksCount >= invisibilityTime + visibilityTime) {
            ticksCount = 0;
        }
        Player player = observer.getPlayerP();
        if (getPosition().range(player.getPosition()) < 2) {
            int attackRoll = attack();
            messageCallback.send(getName() + " rolled " + attackRoll + " attack points");
            int defenseRoll = player.defend();
            messageCallback.send(player.getName() + " rolled " + defenseRoll + " defense points");
            int damage = Math.max(0, attackRoll - defenseRoll);
            messageCallback.send(getName() + " dealt " + damage + " damage to " + player.getName());
            player.getHealth().takeDamage(damage);
            if (!player.alive()) {
                player.onDeath();
            }
        }
    }
    @Override
    public boolean visiable() {
        return visible;
    }
    public String toString(){
        if(visible)
            return String.valueOf(tile);
        return".";
    }
}
