package test;

import control.initializers.TileFactory;
import model.tiles.units.enemies.Enemy;
import model.tiles.units.players.Player;
import org.junit.jupiter.api.Test;
import utils.Position;
import utils.callbacks.MessageCallback;
import utils.generators.FixedGenerator;

import static org.junit.jupiter.api.Assertions.*;

class TileFactoryTest {
    @Test
    void testProducePlayerById() {
        TileFactory factory = new TileFactory();
        Player player = factory.producePlayer(2);
        assertEquals("The Hound", player.getName());
    }

    @Test
    void testProduceEnemy() {
        TileFactory factory = new TileFactory();
        Enemy e = factory.produceEnemy('k', new Position(1, 2), new FixedGenerator(), msg -> {});
        assertEquals("Lannister Knight", e.getName());
    }
}
