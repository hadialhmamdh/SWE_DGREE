package control;

import control.initializers.LevelInitializer;
import control.initializers.TileFactory;
import model.game.Game;
import model.tiles.units.players.Player;
import utils.callbacks.MessageCallback;
import utils.generators.FixedGenerator;
import utils.generators.Generator;
import utils.generators.RandomGenerator;

import java.io.IOException;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.List;
import java.util.Scanner;

public class Program {
    public static void main(String[] args) {
        if (args.length == 0) {
            System.out.println("Error: No folder path provided.");
            return;
        }
        String folderPath = args[0];
        List<String> files = LevelInitializer.getFilePaths(folderPath);
        Scanner scanner = new Scanner(System.in);
        TileFactory tileFactory = new TileFactory();
        int num = -1;
        while (true) {
            System.out.println("\n Select Your Player:");
            System.out.println(tileFactory.playerType());
            System.out.print("Enter a number (1 to 7): ");
            if (scanner.hasNextInt()) {
                num = scanner.nextInt();
                if (num >= 1 && num <= 7) {
                    break;
                } else {
                    System.out.println("Number out of range. Please try again.");
                }
            } else {
                System.out.println("Invalid input. Please enter a number.");
                scanner.next();
            }
        }
        Generator generator = new RandomGenerator();
        Game game = new Game(files, System.out::println, num, tileFactory, generator);
        game.run();
    }
}


