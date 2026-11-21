package model.tiles.units.players;

import model.tiles.units.HeroicUnit;
import model.tiles.units.Unit;
import model.tiles.units.enemies.Enemy;
import utils.Position;

import java.util.List;

public  abstract class Player extends Unit implements HeroicUnit{
    public static final char PLAYER_TILE = '@';
    protected static final int LEVEL_REQUIREMENT = 50;
    protected static final int HEALTH_GAIN = 10;
    protected static final int ATTACK_GAIN = 4;
    protected static final int DEFENSE_GAIN = 1;

    protected int level;
    protected int experience;

    public Player(String name, int hitPoints, int attack, int defense) {
        super(PLAYER_TILE, name, hitPoints, attack, defense);
        this.level = 1;
        this.experience = 0;
    }

    public void addExperience(int experienceValue){
        this.experience += experienceValue;
        while (experience >= levelRequirement()) {
            levelUp();
            messageCallback.send(getName()+" reached level "+level+":+ "+HEALTH_GAIN*this.level+" Health, +"+ ATTACK_GAIN* this.level+" Attack, +"+this.level+" Defense");

        }
    }

    public void levelUp(){
        this.experience -= levelRequirement();
        this.level++;
        int healthGain = healthGain();
        int attackGain = attackGain();
        int defenseGain = defenseGain();
        health.increaseMax(healthGain);
        health.heal();
        attack += this.attack+attackGain;
        defense += this.defense+defenseGain;
    }

    protected int levelRequirement(){
        return LEVEL_REQUIREMENT * level;
    }

    protected int healthGain(){
        return HEALTH_GAIN * level;
    }

    protected int attackGain(){
        return ATTACK_GAIN * level;
    }

    protected int defenseGain(){
        return DEFENSE_GAIN * level;
    }

    @Override
    public void accept(Unit unit) {
        unit.visit(this);
    }

    public void visit(Player p){
        // Do nothing
    }

    public void visit(Enemy e) {
        if(!(e.visiable()))
            e.visit(this);
        else {

            messageCallback.send(getName() + " engaged in combat with " + e.getName());
            messageCallback.send(description());
            messageCallback.send(e.description());
            int output = battle(e);
            if (!e.alive()) {
                messageCallback.send(e.getName() + " died " + getName() + " gained " + e.experienceValue() + " experience");
                addExperience(e.experienceValue());
                swapPosition(e);
                e.onDeath();
            } else {
               return;
            }
        }
    }

    public int hit(Unit enemy,int damage){
        if(damage >0)
            return enemy.getHealth().takeDamage(damage);
        else
            return damage;
    }
    public void specialHit(Enemy e,int damage) {
        int output =hit(e,damage);
        if(!e.alive()){
            messageCallback.send(e.getName()+" died. "+getName()+" gained "+e.experienceValue()+" experience");
            addExperience(e.experienceValue());
            e.onDeath();
        }

    }
    @Override
    public void onDeath() {super.onDeath();}
    public String description() {
        return super.description()+ " level:"+level+" experience:"+experience;
    }

    public abstract void update(List<Enemy> enemies);
    public abstract void notifyNumOfGameTick();
    public void castAbility() {
        specialAbility();
    }
    public abstract void  specialAbility();

    public int getLevel() {
        return level;
    }
}
