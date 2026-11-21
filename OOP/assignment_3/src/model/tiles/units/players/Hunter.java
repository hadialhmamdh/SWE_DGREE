package model.tiles.units.players;

import model.tiles.units.Unit;
import model.tiles.units.enemies.Enemy;
import utils.Health;
import utils.Position;

import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;
import java.util.TreeMap;
import java.util.stream.Collectors;

public class Hunter extends Player {
    private final int INIT_A=10;
    private final int ATTACK_A=2;
    private final int ARROW_A=10;
    private int range;
    private int arrowsCount;
    private int ticksCount;
    private List<Enemy> enemies;

    public Hunter(String name, int hitpoints, int attack, int defense, int range) {
        super(name, hitpoints, attack, defense);
        this.range=range;
        this.arrowsCount=INIT_A;
        this.ticksCount=0;
        this.enemies=new ArrayList<>();
    }

    public void levelUp() {
        super.levelUp();
        this.arrowsCount = this.arrowsCount + ARROW_A * level;
        this.attack = this.attack + (ATTACK_A * level);
        this.defense = this.defense + level;
    }

    @Override
    public void update(List<Enemy> enemies) {
        this.enemies=enemies;
    }


    @Override
    public void notifyNumOfGameTick() {
        if(this.ticksCount==10){
            this.arrowsCount=this.arrowsCount+level;
            this.ticksCount=0;
        } else
            this.ticksCount=this.ticksCount+1;
    }
    public int attack() {
        return this.attack;
    }

    @Override
    public void specialAbility() {
        Position myPos = getPosition();
        List<Enemy> inRange = enemies.stream()
                .filter(e -> e.visiable() && myPos.range(e.getPosition()) < range)
                .collect(Collectors.toList());
        if (arrowsCount == 0 || inRange.isEmpty()) {
            if (inRange.isEmpty()) {
                messageCallback.send(getName() + " tried to shoot an arrow but there were no enemies in range");
            } else {
                messageCallback.send(getName() + " tried to shoot an arrow but has no arrows left");
            }
            return;
        }
        inRange.sort(Comparator.comparingDouble(e -> myPos.range(e.getPosition())));
        Enemy target = inRange.get(0);
        messageCallback.send(getName() + " fired an arrow at " + target.getName());
        arrowsCount--;
        int defenseRoll = target.defend();
        messageCallback.send(target.getName() + " rolled " + defenseRoll + " defense points");
        int damage = Math.max(0, attack - defenseRoll);
        messageCallback.send(getName() + " hits " + target.getName() + " for " + damage + " ability damage");
        specialHit(target, damage);
    }
    public String description() {
        return super.description()+" Arrows: "+arrowsCount+" Range: "+range;
    }

}
