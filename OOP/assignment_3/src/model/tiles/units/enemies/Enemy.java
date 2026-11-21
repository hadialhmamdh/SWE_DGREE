package model.tiles.units.enemies;

import model.tiles.units.Unit;
import model.tiles.units.players.Player;

public abstract class Enemy extends Unit {
    protected int experienceValue;

    public Enemy(char tile, String name, int hitPoints, int attack, int defense, int experienceValue) {
        super(tile, name, hitPoints, attack, defense);
        this.experienceValue = experienceValue;
    }

    public int experienceValue() {
        return experienceValue;
    }

    @Override
    public void accept(Unit unit) {
        unit.visit(this);
    }

    public void visit(Enemy e){
        // Do nothing
    }

    public void visit(Player p) {
        messageCallback.send(getName()+" engaged in combat with "+p.getName());
        battle(p);
        if (!p.alive()){
            p.onDeath();
        }
        else {
            messageCallback.send(description());
            messageCallback.send(p.description());
        }
    }
    public void onDeath(){
        observer.remove(this);
    }
    public abstract void onTurn();
    public abstract boolean visiable();

}
