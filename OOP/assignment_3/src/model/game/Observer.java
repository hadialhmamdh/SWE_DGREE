package model.game;

import model.tiles.Tile;
import model.tiles.units.enemies.Enemy;
import model.tiles.units.players.Player;
import utils.Position;

public interface Observer {
    public void update(Tile t1,Tile t2);
    public Player getPlayerP();
    public Tile getTileP(Position position);
    public void remove(Enemy enemy);
}
