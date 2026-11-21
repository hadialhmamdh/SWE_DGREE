package model.tiles.units.players;

import model.tiles.units.enemies.Enemy;
import utils.Health;
import utils.Position;
import utils.generators.Generator;
import utils.generators.RandomGenerator;

import java.util.List;
import java.util.stream.Collectors;

public class Mage extends Player {
    private final int MANA_POOL_P = 25;
    private final int SPELL_POWER_P = 10;
    private int manaPool;
    private int currentPool;
    private int manaCost;
    private int spellPower;
    private int hitsCount;
    private int abilityRange;
    private List<Enemy> enemies;

    public Mage(String name, int health, int attack, int defense, int manaPool, int spellPower, int manaCost, int hitsCount, int abilityRange) {
        super(name, health, attack, defense);
        this.manaPool = manaPool;
        this.currentPool = manaPool / 4;
        this.spellPower = spellPower;
        this.manaCost = manaCost;
        this.hitsCount = hitsCount;
        this.abilityRange = abilityRange;

    }
    public void levelUp() {
        super.levelUp();
        this.manaPool = (this.manaPool) + manaPoolUpdate();
        this.currentPool = Math.min(this.currentPool + (this.manaPool / 4), this.currentPool);
        this.spellPower = this.spellPower + spellPowerUpdate();
        messageCallback.send( "+ " +manaPoolUpdate()+" maximum mana, +"+spellPowerUpdate()+" spell power");
    }
    public int manaPoolUpdate(){
        return MANA_POOL_P * level;
    }
    public int spellPowerUpdate(){
        return SPELL_POWER_P * level;
    }

    @Override
    public void update(List<Enemy> enemies){
        this.enemies=enemies;
    }


    @Override
    public void notifyNumOfGameTick() {
        this.currentPool=Math.min(manaPool,currentPool+level);
    }

    @Override
    public void specialAbility() {
        if (manaCost > this.currentPool)
            messageCallback.send(getName()+" tried to cast Blizzard, but there was not enough mana: "+currentPool+"/"+manaCost);
        else {
            messageCallback.send(getName()+" cast Blizzard");
            Position p = getPosition();
            List<Enemy> enemyList = (enemies).stream().filter(x ->(p.range(x.getPosition()) < abilityRange )&& (x.visiable())).collect(Collectors.toList());
            int hitC = 0;
            int index;
            while ((hitC < this.hitsCount) && (!enemyList.isEmpty())) {
                index = generator.generate(enemyList.size());
                    Enemy enemy = enemyList.get(index);
                    int randomDefense = enemy.defend();
                    messageCallback.send(enemy.getName()+" rolled "+randomDefense+" defense points");
                    messageCallback.send(getName()+" hit "+enemy.getName()+" for "+Math.max(spellPower-randomDefense,0)+ " ability damage");
                    specialHit(enemy, this.spellPower-randomDefense);
                    enemyList.remove(index);
                hitC = hitC + 1;
            }
            this.currentPool = this.currentPool - this.manaCost;
        }
    }
    public String description() {
        return super.description()+" Mana:"+currentPool+"/"+manaPool+" Spell Power:"+spellPower;
    }
}
