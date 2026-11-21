package model.tiles.units.enemies;

import model.tiles.units.Unit;
import utils.Health;
import utils.Position;

import java.util.Random;

public class Monster extends Enemy {
    protected int visionRange;

    public Monster(char enemySympol, String name, int hitpoints, int attack, int defense, int experienceValue, int visionRange){
        super(enemySympol,name,hitpoints, attack, defense,experienceValue);
        this.visionRange=visionRange;
    }
    @Override
    public void onTurn(){
        Position player = observer.getPlayerP().getPosition();
        int range=(int)(getPosition().range(player));
        int dx= getPosition().getX()-player.getX();
        int dy= getPosition().getY()-player.getY();
        int x=getPosition().getX();
        int y=getPosition().getY();
        if(range< this.visionRange){
            if (Math.abs(dx) > Math.abs(dy)) {
                if(dx>0)
                    interact(observer.getTileP(new Position(x-1,y)));
                else
                    interact(observer.getTileP(new Position(x+1,y)));
            }
            else{
                if(dy>0)
                    interact(observer.getTileP(new Position(x,y-1)));
                else
                    interact(observer.getTileP(new Position(x,y+1)));
            }
        }
        else
            randomAction();
    }

    public void randomAction() {
        int x = getPosition().getX();
        int y = getPosition().getY();
        int index = generator.generate(5);
        if (index == 0) {
            interact(observer.getTileP(new Position(x - 1, y)));
        } else if (index == 1) {
            interact(observer.getTileP(new Position(x + 1, y)));
        } else if (index == 2) {
            interact(observer.getTileP(new Position(x, y - 1)));
        } else if (index == 3) {
            interact(observer.getTileP(new Position(x, y + 1)));
        } else if (index == 4) {
        }
    }

    @Override
    public boolean visiable() {
        return true;
    }
    public String description() {
        return super.description()+" vision Range: "+visionRange;
    }
}
