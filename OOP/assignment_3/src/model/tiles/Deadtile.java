package model.tiles;

import model.tiles.units.Unit;

public class Deadtile extends Tile {
    public static final char DEAD_TILE='X';

    public Deadtile(){
        super(DEAD_TILE);
    }
    @Override
    public void accept(Unit unit) {}
}
