package model.tiles.units.players;

import model.tiles.units.Unit;
import model.tiles.units.enemies.Enemy;
import utils.Health;
import utils.Position;

import java.util.ArrayList;
import java.util.List;
import java.util.stream.Collectors;

public class Rogue extends Player {
    private int cost;
    private int currentEnergy;
    public static final int ATTACK_NEWP=3;
    private List<Enemy> enemies;

    public Rogue (String name, int hitpoints, int attack, int defense, int cost){
        super(name,hitpoints,attack,defense);
        this.cost=cost;
        this.currentEnergy=100;
        this.enemies=new ArrayList<>();
    }

    public void levelUp() {
        super.levelUp();
        this.currentEnergy=100;
        this.attack = ATTACK_NEWP*this.level;
    }

    @Override
    public void update(List<Enemy> enemies){
        this.enemies=enemies;
    }

    @Override
    public void notifyNumOfGameTick() {
        this.currentEnergy=Math.min(this.currentEnergy+10,100);
    }

    @Override
    public void specialAbility() {
        if (this.cost > this.currentEnergy) {
            messageCallback.send(getName() + " tried to cast Fan of Knives, but there was not enough energy: " + currentEnergy + "/" + cost);
            return;
        }
        messageCallback.send(getName() + " cast Fan of Knives");
        Position pos = getPosition();
        List<Enemy> enemiesInRange = enemies.stream()
                .filter(e -> pos.range(e.getPosition()) < 2 && e.visiable())
                .collect(Collectors.toList());
        for (Enemy enemy : enemiesInRange) {
            int defenseRoll = enemy.defend();
            messageCallback.send(enemy.getName() + " rolled " + defenseRoll + " defense points");
            int damage = attack - defenseRoll;
            messageCallback.send(getName() + " hit " + enemy.getName() + " for " + Math.max(damage, 0) + " ability damage");
            specialHit(enemy, damage);
        }
        currentEnergy -= cost;
    }
    public String description() {
        return super.description()+" Cost:"+cost+" Current Energy:"+currentEnergy;
    }

}
