package control.initializers;

import model.game.Board;
import model.tiles.Tile;
import model.tiles.units.enemies.Enemy;
import model.tiles.units.players.Player;
import utils.Position;
import utils.callbacks.MessageCallback;
import utils.generators.Generator;

import java.io.IOException;
import java.nio.file.DirectoryStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public class LevelInitializer {
    private TileFactory group;
    private MessageCallback messageCallback;
    private Generator generator;
    private Player player;
    private Board board;

    public LevelInitializer(int playerID,TileFactory group,MessageCallback messageCallback,Generator generator){
        this.group=group;
        this.messageCallback=messageCallback;
        this.generator=generator;
    }
    public void initLevel(String levelPath){
        List<String> lines;
        try {
            lines = Files.readAllLines(Paths.get(levelPath));
        } catch (IOException e) {
            throw new RuntimeException(e);
        }

        List<Tile> tiles = new ArrayList<>();
        List<Enemy> enemies=new ArrayList<>();
        for(int i=0;i<lines.size();i=i+1){
            String temp=lines.get(i);
            for(int j=0;j<temp.length();j=j+1){
                Position p=new Position(i,j);
                switch(temp.charAt(j)) {
                    case '.':
                        Tile tile=group.produceEmpty(p);
                        tiles.add(tile);
                        break;
                    case '#':
                        Tile tile2=group.produceWall(p);
                        tiles.add(tile2);

                        break;
                    case '@':
                        player= group.producePlayer();
                        player.initialize(p,generator,messageCallback);
                        tiles.add(player);
                        break;
                    default:
                        Enemy tile4= group.produceEnemy(temp.charAt(j),p,this.generator,this.messageCallback);
                        tile4.initialize(p,generator,messageCallback);
                        enemies.add(tile4);
                        tiles.add(tile4);
                        break;
                }
            }
        }
        int width= lines.isEmpty() ? 0 : lines.get(0).length();
        int height=lines.size();
        board=new Board(tiles,player,enemies,width,height);
        board.Init();
    }
    public Board getBoard(){
        return this.board;
    }
    public void Init(){
        getBoard().Init();
        player.update(getBoard().getEnemyList());
        player.deathCallBack(()-> {
            messageCallback.send("You lost.");
            getBoard().remove();
        });

    }

    public static List<String> getFilePaths(String folderPath) {
        List<String> filePaths = new ArrayList<>();
        Path folder = Paths.get(folderPath);
        try (DirectoryStream<Path> stream = Files.newDirectoryStream(folder)) {
            for (Path entry : stream) {
                if (Files.isRegularFile(entry)) {
                    filePaths.add(entry.toString());
                }
            }
        } catch (IOException e) {
            System.err.println("Error reading the directory: " + e.getMessage());
        }
        return filePaths;
    }
}
