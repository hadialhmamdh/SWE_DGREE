package test;

import control.initializers.TileFactory;
import model.game.Game;
import model.tiles.units.players.Player;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import utils.callbacks.MessageCallback;
import utils.generators.FixedGenerator;

import java.util.Arrays;
import java.util.List;

import static org.junit.jupiter.api.Assertions.*;

class GameTest {
    private TileFactory tileFactory;
    private MessageCallback callback;
    private FixedGenerator generator;
    private Player player;

    @BeforeEach
    void setUp() {
        tileFactory = new TileFactory();
        callback = msg -> {};
        generator = new FixedGenerator();
    }
    @Test
    void testPlayerSelection() {
        Player player = tileFactory.producePlayer(3);
        assertEquals("Melisandre", player.getName());
    }
}
