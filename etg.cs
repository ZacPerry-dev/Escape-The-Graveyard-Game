using System;
using System.IO;

/* TODO: 
- Get the correct entities created 
- Add them to their specific location somehow (that way, you can have more than one entity at a location )
*/


struct Coords {
    public int x;
    public int y;

    public Coords(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(Object obj) {
        return obj is Coords c && this == c;
    }

    public override int GetHashCode() {
        return this.x.GetHashCode() ^ this.y.GetHashCode();
    }

    public static bool operator ==(Coords a, Coords b) {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Coords a, Coords b) {
        return !(a == b);
    }
}

/* Contains a 2d array of Locations */
class Level {
    
    public Location location {get; set;}
    public Location [,] arr; /* Array of locations */
    public int x; 
    public int y;

    public Level(int x, int y) {
        this.arr = new Location[x, y];
        this.x = x;
        this.y = y;
        for (int i = 0; i < this.x; i++) {
            for (int j = 0; j < this.y; j++) {
                this.arr[i,j] = new Location();
            }
        }
    }
}

/* Contains entity objects */
class Location {

    Entity e;

    public Location() {
    }

    public virtual void print(int x, int y){}
}

class ExitLocation : Location {

    Level level;

    public ExitLocation(int x, int y, Level level){
        this.level = level;
    }
    
}


/* Inheritence class from location to represent the exit location in the level. 
Should print the messages about the gate and key according to the reference executable */

abstract class Entity {
    public virtual  void look() {}
    public abstract void interact(Player player);
}

/* Use inheritence and polymorphism to create classes for keys, loot, and skeletons
Should inherit from the entity class and implement the missing functionalities, look & interact */

class Keys : Entity {
    public override void look() {}
    public override void interact(Player player){}
}

class Loot : Entity {
    public override void look() {}
    public override void interact(Player player){}
}

class Skeletons : Entity {
    public override void look() {}
    public override void interact(Player player){}
}


/* Encapsulation should be used. The player class should hold the players location and be responsible for updating it */
class Player {
    public Coords coords { get; set; }
    public Location location {get; set;}

    public Player() {
        this.coords = new Coords(0, 0);
    }

    public bool is_at(Coords xy) {
        return this.coords == xy;
    }

    public bool is_alive() { return true; }

    public bool has_key() { return false; }

    public void print_stats() {
        Console.WriteLine($"  LOCATION: {this.coords.x}, {this.coords.y}");
    }
}

class Game {
    int    num_turns;
    Level  level;
    Location ExitLocation;
    public Player player { get; }

    public Game() {
        this.player = new Player();
    }

    public void load(string path) {
        Console.WriteLine(path);
        Console.WriteLine("\n");

        string line;
        using (StreamReader reader = new StreamReader(path)) {
            while ((line = reader.ReadLine()) != null) {
                if (line == "") { continue; }

                string[] split = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (split.Length < 3) {
                    Console.WriteLine($"Bad command in level file: '{line}'");
                    Environment.Exit(1);
                }

                int x     = int.Parse(split[1]);
                int y     = int.Parse(split[2]);
                int count = 0;

                if (split.Length > 3) {
                    count = int.Parse(split[3]);
                }

                switch (split[0]) {
                    case "size":
                        // Set the level's size to x by y
                        /* Create the 2d array of locations with x & y */
                        this.level = new Level(x, y);
                        break;

                    case "exit":
                        // Set the level's exit location to be x, y
                        this.ExitLocation = new ExitLocation(x,y, this.level);
                        this.level.arr[x,y] = this.ExitLocation;
                        Console.WriteLine("Exit location in array: " + this.level.arr[x,y] + "\n");
                        break;

                    case "key":
                        // Add a key to location x, y
                        /* Key locations */
                        break;

                    case "loot":
                        // Add loot to location x, y with count coins
                        /* Create loot locations */
                        break;

                    case "skeleton":
                        // Add a skeleton to location x, y
                        /* Create skeleton locations */
                        break;

                    default:
                        Console.WriteLine($"Bad command in level file: '{line}'");
                        Environment.Exit(1);
                        break;

                }
            }
        }
    }

    public void input(string line) {
        this.num_turns += 1;

        // Check for exhaustion?
        /* Check if this.num_turns == the size (w*h*2) of the map */
        /* This is from the level part of the input file */

        Console.WriteLine("================================================================");

        string[] split = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (split.Length != 2) {
            Console.WriteLine($"Bad command in input: '{line}'");
            return;
        }

        Coords new_coords = this.player.coords;
        switch (split[1]) {
            case "north":
                new_coords.y += 1;
                break;
            case "south":
                new_coords.y -= 1;
                break;
            case "east":
                new_coords.x += 1;
                break;
            case "west":
                new_coords.x -= 1;
                break;
            default:
                Console.WriteLine($"Bad command in input: '{line}'");
                return;
        }

        // Are the new coords valid?
        switch (split[0]) {
            case "go":
                this.player.coords = new_coords;
                // Need to look at the new location and interact with it.
                break;
            case "look":
                // Need to look at the location.
                /* Will look and see what is at that location by looping through the array */
                /* Depending on what is there, we will print the specific prompt based on the entity at this location */
              
                Console.WriteLine("Coords: " + this.level.arr[new_coords.x, new_coords.y]);
                /* Call print for the specific location at the specific coords */
                if (this.level.arr[new_coords.x, new_coords.y] is ExitLocation) {Console.WriteLine("POGGGGGERRRRS");}
                break;
            default:
                Console.WriteLine($"Bad command in input: '{line}'");
                return;
        }
    }

    bool is_over() {
        // What are the exit conditions?
        return false;
    }

    void print_stats() {
        if (this.is_over() && player.is_alive()) {
            Console.WriteLine("You successfully escaped the graveyard!");
        } else {
            Console.WriteLine("You did not escape the graveyard. GAME OVER");
        }
        Console.WriteLine($"Game ended after {this.num_turns} turn(s).");
        player.print_stats();
    }

    public void exit() {
        Console.WriteLine("================================================================");
        this.print_stats();
        Environment.Exit(0);
    }

    public void exit_if_over() {
        if (this.is_over()) { this.exit(); }
    }

    public void intro() {
        Console.WriteLine("You awake in a daze to find yourself alone in the dead of night, surrounded by headstones...");
        Console.WriteLine("You must escape this graveyard.");
        Console.WriteLine("================================================================");
        // Look at the current location.
        Console.Write($"{this.player.coords.x}, {this.player.coords.y}> ");
    }
}

class ETG {
    static void Main(string[] args) {
        if (args.Length != 1) {
            Console.WriteLine("ERROR: expected a single argument (the level file)");
            Environment.Exit(1);
        }

        Game game = new Game();

        game.load(args[0]);
        game.intro();

        game.exit_if_over();

        string line;

        while ((line = Console.ReadLine()) != null) {
            if (line == "") { continue; }
            game.input(line);
            game.exit_if_over();
            Console.Write($"{game.player.coords.x}, {game.player.coords.y}> ");
        }

        game.exit();
    }
}
