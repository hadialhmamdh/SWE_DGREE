package model.tiles.units.players;

import model.tiles.units.enemies.Enemy;
import utils.Position;

import java.util.ArrayList;
import java.util.List;
import java.util.stream.Collectors;

public class Warrior extends Player {
    private int abilityCooldown;
    private int remainingCooldown;
    private final int HEALTH_W=5;
    private final int  ATTACK_W=2;
    private final double PERCENT=0.1;
    private final int SPECIAL=10;
    private List<Enemy> enemies;

    public Warrior(String name, int hitPoints, int attack, int defense , int abilityCooldown) {
        super(name, hitPoints, attack, defense);
        this.abilityCooldown= abilityCooldown;
        this.remainingCooldown=0;
        this.enemies = new ArrayList<>();
    }

    public void update(List<Enemy> enemies){
        this.enemies=enemies;
    }

    @Override
    public void levelUp() {
        super.levelUp();
        this.remainingCooldown = 0;
        health.increaseMax(HEALTH_W * level) ;
        health.heal();
        this.attack += ATTACK_W * level;
        this.defense += 1 * level;
        messageCallback.send(" remainingCooldown: "+remainingCooldown);
    }


    public String description() {
        return super.description()+" abilityCooldown:"+abilityCooldown+" remainingCooldown:"+Math.min(remainingCooldown,abilityCooldown);
    }
    public void specialAbility() {
        if (remainingCooldown > 0)
            messageCallback.send(getName() + " tried to cast Avenger’s Shield but there is a cooldown " + remainingCooldown + "/" + abilityCooldown);
        else {
            int update = (SPECIAL * defense) + getHealth().getCurrent();
            health.setCurrent(Math.min(update, getHealth().getCapacity()));
            messageCallback.send(getName()+" used  Avenger’s Shield, healing for "+SPECIAL * defense);
            Position p = getPosition();
            // git enemies closest than 3
            List<Enemy> enemyList3 = (enemies).stream().filter(x ->(p.range(x.getPosition()) < 3 )&& (x.visiable())).collect(Collectors.toList());
            if(!(enemyList3.isEmpty())){
                int index = generator.generate(enemyList3.size());
                int damage = (int) ((health.getCapacity()) * PERCENT);
                Enemy enemy = enemyList3.get(index);
                messageCallback.send(getName()+" hits "+enemy.getName()+" for "+damage+" ability damage");
                specialHit(enemy, damage);}
            this.remainingCooldown = this.abilityCooldown+1;
        }
    }

    public void notifyNumOfGameTick() {
        remainingCooldown=Math.max(0,remainingCooldown-1);
    }
    public int getRemainingCooldown(){
        return remainingCooldown;
    }
}
