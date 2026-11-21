package model.tiles;

import model.tiles.units.Unit;
import utils.Position;

import model.game.Observer;

public abstract class Tile {
    protected char tile;
    protected Position position;
    protected Observer observer;

    public Tile(char tile){
        this.tile = tile;
    }

    public Tile initialize(Position p){
        this.position = p;
        return this;
    }
    public void observerInit(Observer observer){
        this.observer=observer;
    }

    public void swapPosition(Tile t) {
        Position temp = t.position;
        t.position = this.position;
        this.position = temp;
        observer.update(this,t);
    }

    @Override
    public String toString() {
        return String.valueOf(tile);
    }

    public abstract void accept(Unit unit);

    public Position getPosition() {
        return position;
    }
}
