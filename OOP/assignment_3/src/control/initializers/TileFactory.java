package control.initializers;

import model.game.Board;
import model.tiles.Empty;
import model.tiles.Tile;
import model.tiles.Wall;
import model.tiles.units.enemies.Boss;
import model.tiles.units.enemies.Enemy;
import model.tiles.units.enemies.Monster;
import model.tiles.units.enemies.Trap;
import model.tiles.units.players.*;
import utils.Health;
import utils.Position;
import utils.callbacks.DeathCallback;
import utils.callbacks.MessageCallback;
import utils.generators.Generator;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Map;
import java.util.function.Supplier;

public class TileFactory {
    private Player player;

    private  final List<Supplier<Player>> playerTypes = Arrays.asList(
            () -> new Warrior("Jon Snow",300, 30, 4,3),
            () -> new Warrior("The Hound", 400, 20, 6,5),
            () -> new Mage("Melisandre",100,5,1,300,15,30,5,6),
            () -> new Mage("Thoros of Myr",250,24,4,150,20,20,3,4),
            () -> new Rogue("Arya Stark",150,40,2,20),
            () -> new Rogue("Bronn",250,35,3,50),
            ()-> new Hunter("Ygritte",220,30,2,6)
    );
    private  final Map<Character, Supplier<Enemy>> enemyTypes = Map.ofEntries(
            Map.entry('s', () -> new Monster('s', "Lannister Soldier", 80, 8, 3, 25, 3)),
            Map.entry('k', () -> new Monster('k', "Lannister Knight", 200, 14, 8, 50, 4)),
            Map.entry('q', () -> new Monster('q', "Queen’s Guard", 400, 20, 15, 100, 5)),
            Map.entry('z', () -> new Monster('z', "Wright", 600, 30, 15, 100, 3)),
            Map.entry('b', () -> new Monster('b', "Bear-Wright", 100, 75, 30, 250, 4)),
            Map.entry('g', () -> new Monster('g', "Giant-Wright", 1500, 100, 40, 500, 5)),
            Map.entry('w', () -> new Monster('w', "White Walker", 2000, 150, 50, 1000, 6)),
            Map.entry('M', () -> new Boss('M', "The Mountain", 1000, 60, 25, 500, 6,5)),
            Map.entry('C', () -> new Boss('C', "Queen Cersei",100, 10, 10, 1000, 1,8)),
            Map.entry('K', () -> new Boss('K', "Night’s King", 5000, 300, 150, 5000, 8,3)),
            Map.entry('B', () -> new Trap('B', "Bonus Trap",1, 1, 1, 250, 1, 5)),
            Map.entry('Q', () -> new Trap('Q', "Queen's Trap",250, 50, 10, 100, 3, 7)),
            Map.entry('D', () -> new Trap('D', "Death Trap", 500, 100, 20, 250, 1, 10))
    );
    public Player producePlayer(int playerID){
        Supplier<Player> supp = playerTypes.get(playerID-1);
        this.player = supp.get();
        return this.player;

    }

    public Player producePlayer(){
        return this.player;
    }
    public Enemy produceEnemy(char tile, Position p, Generator g, MessageCallback m){
        Enemy e = enemyTypes.get(tile).get();
        e.initialize(p,g,m);
        return e;
    }
    public Tile produceEmpty(Position p){
        Tile empty=new Empty();
        empty.initialize(p);
        return empty;
    }

    public Tile produceWall(Position p){
        Tile wall=new Wall();
        wall.initialize(p);
        return wall;
    }
    public String playerType(){
        List<String> list=new ArrayList<>();
        StringBuilder s=new StringBuilder();
        list.add("1 to : Jon Snow   Health:300/300   Attack:30   Defense:4   Level:1   Experience:0/50   Cooldown:0/3");
        list.add("2 to : The Hound   Health:400/400   Attack:20   Defense:6   Level:1   Experience:0/50   Cooldown:0/5");
        list.add("3 to : Melisandre   Health:100/100   Attack:5   Defense:1   Level:1   Experience:0/50   mana:75/300   Cost:30  spell Power:15   Hit Count:5   Range:6");
        list.add("4 to: Thoros of Myr   Health:250/250   Attack:25   Defense:4   Level:1   Experience:0/50   mana:37/150   Cost:20   spell Power:20   Hit Count:3   Range:4");
        list.add("5 to: Arya Stark   Health:150/150   Attack:40   Defense:2   Level:1   Experience:0/50   Energy:100/100   Cost:20");
        list.add("6 to: Bronn  Health:250/250   Attack:35   Defense:3   Level:1   Experience:0/50   Energy:100/100   Cost:50");
        list.add("7 to: Ygritte   Health:220/220   Attack:30   Defense:2   Level:1   Experience:0/50   Arrows:10   Range:6");
        for (int i=0;i< list.size();i=i+1){
            s.append(list.get(i));
            s.append("\n");
        }
        return s.toString();
    }
}
