package test;

import model.tiles.units.players.Mage;
import model.tiles.units.players.Rogue;
import model.tiles.units.players.Warrior;
import org.junit.After;
import org.junit.Assert;
import utils.Health;
import utils.Position;
import utils.generators.FixedGenerator;
import utils.callbacks.MessageCallback;
import model.tiles.units.enemies.Monster;
import org.junit.Before;
import org.junit.Test;

import java.io.ByteArrayOutputStream;
import java.io.PrintStream;

import static org.junit.Assert.*;

public class PlayerTest {
    private Warrior warrior;
    private Rogue rogue;
    private Mage mage;
    private Monster monster;



    @Before
    public void Init(){
        warrior=new Warrior("Jon Snow",300,30,4,3);
        mage=new Mage("Melisandre",100,5,1,300,15,30,3,6);
        rogue=new Rogue("Bronn",250,35,3,50);
        monster=new Monster('s',"Lannister Solider",80,8,3,25,3);
        warrior.initialize(new Position(1,2),new FixedGenerator(),(System.out::println));
        mage.initialize(new Position(2,4),new FixedGenerator(), System.out::println);
        rogue.initialize(new Position(3,6),new FixedGenerator(), System.out::println);
        monster.initialize(new Position(2,7),new FixedGenerator(), System.out::println);
    }



    @Test
    public void levelUp() {
        Assert.assertEquals("Jon Snow level is 1",1,warrior.getLevel());
        warrior.addExperience(50);
        Assert.assertEquals("Jon Snow level is 2",2,warrior.getLevel());
        mage.addExperience(150);
        Assert.assertEquals("Melisandre level is 3",3,mage.getLevel());
        rogue.addExperience(3);
        Assert.assertEquals("Bornn level is 1",1,rogue.getLevel());

    }
    @Test
    public void specialAbility() {
        Assert.assertEquals("Jon Snow can use the Avenger’s Shield ability",true,warrior.getRemainingCooldown()==0);
        warrior.specialAbility();
        Assert.assertEquals("Jon Snow can't use the Avenger’s Shield ability",false,warrior.getRemainingCooldown()==0);
    }
    @Test
    public void visit(){
        mage.visit(monster);
        Assert.assertEquals(" Lannister Solider health amount after the battle is 79",monster.getHealth().getCurrent(),79);
        monster.visit(warrior);
        Assert.assertEquals(" Jon Snow  health amount after the battle is 298",warrior.getHealth().getCurrent(),298);
        rogue.visit(monster);
        Assert.assertEquals(" Lannister Solider health amount after the battle is 79",monster.getHealth().getCurrent(),63);
    }
}