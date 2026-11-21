package model.game;

import control.initializers.LevelInitializer;
import control.initializers.TileFactory;
import model.tiles.units.players.Player;
import utils.callbacks.MessageCallback;
import utils.generators.Generator;

import java.util.List;

public class Game {
    protected List<String> list;
    protected Player player;
    protected MessageCallback messageCallback;
    protected int index;
    protected TileFactory tileFactory;
    protected int playerId;
    protected Generator generator;

    public Game(List<String> list, MessageCallback messageCallback, int playerId, TileFactory tileFactory,Generator generator) {
        this.list=list;
        this.messageCallback=messageCallback;
        this.index=0;
        this.playerId=playerId;
        this.tileFactory=tileFactory;
        this.generator=generator;
        this.player = tileFactory.producePlayer(playerId);
        messageCallback.send("you have selected:" + player.getName());
    }
    public void  run(){
        while(player.alive()&&(this.index<list.size())){
            Level i = new Level(uploadLevelBoard(index),messageCallback);
            messageCallback.send(i.toString());
            i.Start();
            index++;
            if (player.alive() && (index + 1 < list.size())) {
                messageCallback.send("You won this level!, now level " + (index + 1) + " is available");
            }
        }
        if(player.alive())
            messageCallback.send("You won the game!");

    }
    public Board uploadLevelBoard(int i){
        LevelInitializer level=new LevelInitializer(playerId,tileFactory,messageCallback,generator);
        level.initLevel(list.get(i));
        level.Init();
        Board board = level.getBoard();
        return board;
    }
}
