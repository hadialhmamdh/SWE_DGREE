package model.tiles.units;

import model.tiles.Empty;
import model.tiles.Tile;
import model.tiles.Wall;
import model.tiles.units.enemies.Enemy;
import model.tiles.units.players.Player;
import utils.Health;
import utils.Position;
import utils.callbacks.DeathCallback;
import utils.callbacks.MessageCallback;
import utils.generators.Generator;

public abstract class Unit extends Tile {
    protected String name;
    protected Health health;
    protected int attack;
    protected int defense;

    protected Generator generator;
    protected DeathCallback deathCallback;
    protected MessageCallback messageCallback;

    public Unit(char tile, String name, int hitPoints, int attack, int defense) {
        super(tile);
        this.name = name;
        this.health = new Health(hitPoints);
        this.attack = attack;
        this.defense = defense;
    }

    public void initialize(Position p, Generator generator,MessageCallback messageCallback){
        super.initialize(p);
        this.generator = generator;
        this.messageCallback=messageCallback;
    }
    public void deathCallBack(DeathCallback deathCallback){
        this.deathCallback=deathCallback;
    }

    public int attack(){
        return generator.generate(attack);
    }

    public int defend(){
        return generator.generate(defense);
    }

    public boolean alive(){
        return health.getCurrent() > 0;
    }

    public int battle(Unit enemy){
        int a1=attack();
        messageCallback.send(getName()+" rolled "+a1+" attack points");
        int a2= enemy.defend();
        messageCallback.send(enemy.getName()+" rolled "+a2+" defense points");
        int damagetaken=a1-a2;
        messageCallback.send(getName()+" dealt "+Math.max(damagetaken,0)+" damage to "+enemy.getName());
        if(damagetaken >0){
            damagetaken=enemy.health.takeDamage(damagetaken);
        }
        return damagetaken;
    }

    public void interact(Tile t){
        t.accept(this);
    }

    public void visit(Empty e){
        swapPosition(e);
    }

    public void visit(Wall w){
        // Do nothing
    }

    public abstract void visit(Player p);
    public abstract void visit(Enemy e);

    public void onDeath(){
        deathCallback.onDeath();
    }

    public String getName() {
        return name;
    }

    public String description (){
        return getName()+" Health:"+health.getCurrent()+"/"+ health.getCapacity()+" Attack point:"+attack+" Defense point:"+defense;
    }
    public Health getHealth(){
        return this.health;
    }
}
