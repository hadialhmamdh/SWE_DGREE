package model.game;

import model.tiles.Deadtile;
import model.tiles.Empty;
import model.tiles.Tile;
import model.tiles.units.enemies.Enemy;
import model.tiles.units.players.Player;
import utils.Position;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;

public class Board implements Observer {
    private Tile[][] board;
    private Player player;
    private List<Enemy> enemies;
    private int width;
    private int height;

    public Board(List<Tile> tiles, Player p, List<Enemy> enemies, int width, int height) {
        this.player = p;
        this.enemies = enemies;
        this.width = width;
        this.height = height;
        this.board = new Tile[height][width];
        for (Tile t : tiles) {
            board[t.getPosition().getX()][t.getPosition().getY()] = t;
        }
    }
    @Override
    public String toString() {
        StringBuilder sb = new StringBuilder();
        for (int x = 0; x < height; x++) {
            for (int y = 0; y < width; y++) {
                Position position = new Position(x, y);
                Tile tile = board[position.getX()][position.getY()];
                if (tile != null) {
                    sb.append(tile.toString());
                } else {
                    sb.append(' ');
                }
            }
            sb.append('\n');
        }
        return sb.toString();
    }



    @Override
    public void update(Tile t1, Tile t2) {
        Position a1 = t1.getPosition();
        Position a2 = t2.getPosition();
        board[a1.getX()][a1.getY()] = t1;
        board[a2.getX()][a2.getY()] = t2;
    }


    @Override
    public Player getPlayerP() {
        return this.player;
    }

    @Override
    public Tile getTileP(Position position) {
        return board[position.getX()][position.getY()];
    }

    @Override
    public void remove(Enemy enemy) {
        Tile em = new Empty();
        em.initialize(new Position(enemy.getPosition().getX(), enemy.getPosition().getY()));
        board[enemy.getPosition().getX()][enemy.getPosition().getY()] = em;
        enemies.remove(enemy);
    }

    public List<Enemy> getEnemyList() {
        return this.enemies;
    }

    public Player getPlayer() {
        return this.player;
    }

    public void Init() {
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                board[i][j].observerInit(this);
            }
        }
    }

    public void remove() {
        board[player.getPosition().getX()][player.getPosition().getY()] = new Deadtile();
    }

    public Tile[][] getBoard() {
        return this.board;
    }
}
