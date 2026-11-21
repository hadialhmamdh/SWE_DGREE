package model.game;

import model.tiles.Tile;
import model.tiles.units.enemies.Enemy;
import model.tiles.units.players.Player;
import utils.Position;
import utils.callbacks.MessageCallback;

import java.util.List;
import java.util.Scanner;

public class Level {
    private Player player;
    private Board board;
    private List<Enemy> enemies;
    private MessageCallback messageCallback;
    Scanner scanner;

    public Level(Board board,MessageCallback messageCallback){
        this.board=board;
        this.player= board.getPlayer();
        this.enemies=board.getEnemyList();
        this.messageCallback=messageCallback;
        scanner=new Scanner(System.in);
    }
    public void Start(){
        while((player.alive())&&(!(enemies.isEmpty()))){
            GameTick();
            player.notifyNumOfGameTick();
            messageCallback.send(board.toString());
            enemies=board.getEnemyList();
        }
    }
    public void GameTick() {
        boolean validInput = false;

        while (!validInput) {
            messageCallback.send(player.description());
            char c = scanner.next().charAt(0);

            int x = player.getPosition().getX();
            int y = player.getPosition().getY();

            switch (c) {
                case 'w':
                    player.interact(board.getTileP(new Position(x - 1, y)));
                    validInput = true;
                    break;
                case 's':
                    player.interact(board.getTileP(new Position(x + 1, y)));
                    validInput = true;
                    break;
                case 'a':
                    player.interact(board.getTileP(new Position(x, y - 1)));
                    validInput = true;
                    break;
                case 'd':
                    player.interact(board.getTileP(new Position(x, y + 1)));
                    validInput = true;
                    break;
                case 'e':
                    player.specialAbility();
                    validInput = true;
                    break;
                case 'q':
                    messageCallback.send("do nothing");
                    validInput = true;
                    break;
                default:
                    messageCallback.send("Invalid input! Please choose one of: w, a, s, d, e, q.");
            }
        }
        this.enemies=board.getEnemyList();
        Tile t=(board.getBoard()[player.getPosition().getX()][player.getPosition().getY()]);
        player.interact(t);
        for(int i=0;i<enemies.size()&& player.alive();i=i+1){
            enemies.get(i).onTurn();
        }
        this.enemies=board.getEnemyList();

    }
        public String toString(){
        return board.toString();
    }
}
