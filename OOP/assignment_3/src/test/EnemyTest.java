package test;

import model.tiles.units.enemies.Monster;
import model.tiles.units.enemies.Trap;
import model.tiles.units.players.Player;
import model.tiles.units.players.Rogue;
import model.tiles.units.players.Warrior;
import org.junit.Assert;
import org.junit.Before;
import org.junit.Test;
import utils.Health;
import utils.Position;
import utils.generators.FixedGenerator;

import static org.junit.Assert.*;

public class EnemyTest {
    private Trap trap1;
    private Trap trap2;
    private Monster monster;
    private Player player1;
    private Player player2;
    @Before
    public void Init(){
        trap1=new Trap('B',"Bonus Trap",1,1,1,250,1,5);
        trap2=new Trap('Q',"Queen Trap",250,50,10,100,3,7);
        monster=new Monster('z',"Wright",600,30,15,100,3);
        player1=new Warrior("Jon Snow",300,30,4,3);
        player2=new Rogue("Arya Stark ",150,40,2,20);
        player1.initialize(new Position(1,2),new FixedGenerator(),(System.out::println));
        player2.initialize(new Position(2,4),new FixedGenerator(), System.out::println);
        trap1.initialize(new Position(3,6),new FixedGenerator(), System.out::println);
        monster.initialize(new Position(1,5),new FixedGenerator(), System.out::println);
        trap2.initialize(new Position(3,4),new FixedGenerator(), System.out::println);

    }
    @Test
    public void visit() {
        monster.visit(player1);
        Assert.assertEquals("Jon Snow health amount should be 287",287,player1.getHealth().getCurrent());
        monster.visit(player2);
        Assert.assertEquals("Arya Stark  health amount should be 136",136,player2.getHealth().getCurrent());
        trap1.visit(player2);
        Assert.assertEquals("Arya Stark  health amount should be 136",136,player2.getHealth().getCurrent());
        trap2.visit(player1);
        Assert.assertEquals("Jon Snow health amount should be 264",264,player1.getHealth().getCurrent());
    }

}