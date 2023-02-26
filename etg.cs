using System;
using System.IO;
using System.Collections.Generic;

/* TODO:
- Move player around (set coordinates / location)

- Refactor
- Make sure everything is OOP 
- Convert some logic and make it less gross

- NOTE: ALways break if at the gate and you dont have key -> Otherwise, it will try to read
into Loot list for the exit location (it doesn't have one)
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
                this.arr[i,j].loot = new List<Loot>();
            }
        }
    }
}

/* Contains entity objects */
class Location {
    public Key key;
    public List<Loot> loot;
    public Skeleton skeleton;

    public Location() {}
    public virtual void print(){}
}

class ExitLocation : Location {
    public Coords coords { get; set; }
    
    public ExitLocation(int x, int y){
        this.coords = new Coords(x, y); 
    }
    public override void print() {
        Console.WriteLine("That looks like the gate out of this spooky place!");
    }

    /* Refactor: Provide look and interact for this maybe -> would be ez and clean */    
}


/* Inheritence class from location to represent the exit location in the level. 
Should print the messages about the gate and key according to the reference executable */

abstract class Entity {
    public virtual  void look() {}
    public abstract void interact(Player player);
}

/* Use inheritence and polymorphism to create classes for keys, loot, and skeletons
Should inherit from the entity class and implement the missing functionalities, look & interact */

class Key : Entity {
    public override void look() {
        Console.WriteLine("You see a key on the ground! Might need that to get out of here...");
        
    }
    public override void interact(Player player){
        player.has_key = true;
        Console.WriteLine("You picked up a key!");
    }
}

class Loot : Entity {
    public int coins{get; set;}
    public bool looted{get;set;}

    public override void look() {
        if (!this.looted) {
            Console.WriteLine("You see what looks like the corner of a treasure chest poking out of the ground.");
        }
        else {
            Console.WriteLine($"A treasure chest sits already opened.");
        }
    }
    public override void interact(Player player){
        if (!this.looted) {
            player.has_loot = true; 
            player.number_of_coins += this.coins;
            Console.WriteLine("You open the chest and find " + this.coins + " coins!");
        }
        else {
            Console.WriteLine($"The chest is empty...");
        }
    }
}

class Skeleton : Entity {
    public override void look() {
        Console.WriteLine("Not much to see here.");
    }
    public override void interact(Player player){
        /* Kill player and print shit ig */
        player.is_dead = true; 
        Console.WriteLine("A bony arm juts out of the ground and grabs your ankle!\nYou've been dragged six feet under by a skeleton.");
    }
}


/* Encapsulation should be used. The player class should hold the players location and be responsible for updating it */
class Player {
    public Coords coords { get; set; }
    public Location location {get; set;}
    public bool is_dead{ get; set;}
    public bool has_key { get; set;}
    public bool has_loot { get; set;}
    public int number_of_coins{get; set;}

    public Player() {
        this.coords = new Coords(0, 0);
    }

    public bool is_at(Coords xy) {
        return this.coords == xy;
    }

    public void print_stats() {
        Console.WriteLine($"  LOCATION: {this.coords.x}, {this.coords.y}");
        Console.WriteLine($"  COINS:    {this.number_of_coins}");
        Console.WriteLine($"  KEY:      {this.has_key}");
        Console.WriteLine($"  DEAD:     {this.is_dead}");
    }
}

class Game {
    int    num_turns;
    Level  level;
    Location ExitLocation;
    public Player player { get; }

    public Game() {
        this.player = new Player();
        this.player.is_dead = false; 
        this.player.has_key = false; 
        this.player.has_loot = false; 
        this.player.number_of_coins = 0;
    }

    public void load(string path) {
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
                        // Creates a new exitLocation 
                        // Will store within the level array 
                        this.ExitLocation = new ExitLocation(x, y);
                        this.level.arr[x,y] = this.ExitLocation;
                        break;

                    case "key":
                        // Create a key entity and add it to the location at the specific x,y
                        // Go to this location within the array and add the key entite to this locations                         
                        this.level.arr[x,y].key = new Key();
                        break;

                    case "loot":
                        // Create a loot entity and add location x, y with count coins
                        Loot loot = new Loot();
                        loot.coins = count;
                        loot.looted = false;
                        this.level.arr[x,y].loot.Add(loot);
                        // this.level.arr[x,y].loot.coins = count;
                        break;

                    case "skeleton":
                        // Create a skeleton entity and add to the location at x,y
                        this.level.arr[x,y].skeleton = new Skeleton();
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

        // Check for exhaustion 
        if (this.is_over()) {
            this.player.is_dead = true;
            Console.WriteLine($"You have died from exhaustion.");
            this.exit_if_over();
        }

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
                
                /* Check if the coords are in bounds and valid */
                if (new_coords.x < 0 || new_coords.y < 0 || new_coords.x > this.level.x - 1 || new_coords.y > this.level.y - 1) {
                    Console.WriteLine($"A towering wall is before you. This must be the edge of the graveyard.");
                    break;
                }

                /* Set cords and booleans to detect items */
                this.player.coords = new_coords;
                bool yes_key = false;
                bool yes_loot = false;
                bool yes_skeleton = false;

                /* If at the exit... */
                if (this.level.arr[new_coords.x, new_coords.y] is ExitLocation) {
                    this.level.arr[new_coords.x, new_coords.y].print();
                    this.player.location = this.level.arr[new_coords.x, new_coords.y];
                    if (this.player.has_key) {
                        Console.WriteLine($"You open the gate with your key!");
                        this.exit_if_over();
                    }
                    else {
                        Console.WriteLine($"You try to open the gate, but it's locked. Must need a key...");
                        break;
                    }                                       
                }

                /* If there is a key.. */
                if (this.level.arr[new_coords.x, new_coords.y].key != null) {
                    this.level.arr[new_coords.x, new_coords.y].key.look();
                    yes_key = true;
                }

               
                /* If there is loot and you are not at the exit */
                if (this.level.arr[new_coords.x, new_coords.y].loot.Count != 0 && 
                !(this.level.arr[new_coords.x, new_coords.y] is ExitLocation)) {
                    // this.level.arr[new_coords.x, new_coords.y].loot.look();
                    foreach (var item in  this.level.arr[new_coords.x, new_coords.y].loot) 
                    {
                        item.look();
                    }
                    yes_loot = true;
                }

                /* If there is a skeleton */
                if (this.level.arr[new_coords.x, new_coords.y].skeleton != null) {
                    if (!yes_key && !yes_loot) {
                        this.level.arr[new_coords.x, new_coords.y].skeleton.look();
                        yes_skeleton = true;
                    } else {
                        yes_skeleton = true;
                    }
                }                

                /* Call interact if certain things exist */
                if (yes_key) {
                     this.level.arr[new_coords.x, new_coords.y].key.interact(this.player);
                    this.level.arr[new_coords.x, new_coords.y].key = null;
                }
                if (yes_loot) {
                    // this.level.arr[new_coords.x, new_coords.y].loot.interact(this.player);
                    // this.level.arr[new_coords.x, new_coords.y].loot.looted = true;
                     foreach (var item in  this.level.arr[new_coords.x, new_coords.y].loot) 
                    {
                        item.interact(this.player);
                        item.looted = true;
                    }
                }
                if (yes_skeleton) {
                    this.level.arr[new_coords.x, new_coords.y].skeleton.interact(this.player);
                    this.exit_if_over();
                }
                if (!yes_key && !yes_loot && !yes_skeleton) {
                    Console.WriteLine($"Not much to see here.");
                    
                }
                break;
            case "look":

                /* check bounds */
                if (new_coords.x < 0 || new_coords.y < 0 || new_coords.x > this.level.x - 1 || new_coords.y > this.level.y - 1) {
                    Console.WriteLine($"A towering wall is before you. This must be the edge of the graveyard.");
                    break;
                }

                /* If an exit location */
                if (this.level.arr[new_coords.x, new_coords.y] is ExitLocation) {
                    this.level.arr[new_coords.x, new_coords.y].print();
                    break;
                }
                
                /* If the locations has a key */
                if (this.level.arr[new_coords.x, new_coords.y].key != null) {
                    this.level.arr[new_coords.x, new_coords.y].key.look();
                }
                
                /* If not the exit and the location has loot */
                if (this.level.arr[new_coords.x, new_coords.y].loot.Count != 0 && 
                !(this.level.arr[new_coords.x, new_coords.y] is ExitLocation)) {
                    foreach (var item in  this.level.arr[new_coords.x, new_coords.y].loot) 
                    {
                        item.look();
                    } 
                }
                
                /* If nothing there... */
                else if (this.level.arr[new_coords.x, new_coords.y].loot.Count == 0 && 
                this.level.arr[new_coords.x, new_coords.y].key == null ) {
                    Console.WriteLine("Not much to see here.");
                }
                break;
            default:
                Console.WriteLine($"Bad command in input: '{line}'");
                return;
        }
    }

    bool is_over() {
       
        // Reached Exit with key
        if (this.player.has_key && this.player.location is ExitLocation) return true;
        
        // Exhausted
        else if (this.num_turns > (2*this.level.x*this.level.y)) return true;
        
        // Encountered skeleton
        else if (this.player.is_dead == true) return true;

        return false;
    }

    void print_stats() {
        if (this.is_over() && !player.is_dead) {
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
        Console.WriteLine($"Not much to see here.");
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
