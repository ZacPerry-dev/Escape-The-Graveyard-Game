/*  Project 1: Escape the Graveyard
    Zachary Perry 
    2/26/23
    
    This program uses OOP to create a text-based adventure game
    Goal: To find the key & escape the graveyard without encountering any skeletons
*/

using System;
using System.IO;
using System.Collections.Generic;

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

/*  Level 
    Contains all level information
    Constructor will construct the 2d array of locations (represents the level grid). Also initializes each levels loot list 
    Implements the out_of_bounds method, which checks if given coordinates are in the levels bounds 
    Has variables for x & y coordinates & the Locations 2d array
*/
class Level {
    public Location [,] arr; /* Array of locations */
    public int x; 
    public int y;

    /*  Takes in x & y values
        Creates a new 2d array with those values
        Loops through the array and sets each entry to a new Location class
    */
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

    /* out_of_bounds
        Passed x & y values. Determines if they exist within the grid 
    */
    public bool out_of_bounds(int x, int y) {
        if (x < 0 || y < 0 || x > this.x - 1 || y > this.y - 1) {
            Console.WriteLine($"A towering wall is before you. This must be the edge of the graveyard.");
            return true;
        }
        return false;
    }
}

/*  Location 
    Contains the specific locations entities (key, loot, skeleton)
    Implements location_look() and location_interact() methods
        These methods call respective entity methods if these entities exist at this location
*/
class Location {
    public Key key;
    public List<Loot> loot;
    public Skeleton skeleton;

    public Location() {}

    /* Checks if the location has an entity
        If so, it will call their respective look() methods to output the specific prompt and return true
    */
    public virtual bool location_look() {
        if (this.key != null || this.loot.Count != 0 || this.skeleton != null) {
            if (this.key != null) {
                this.key.look();
            }

            if (this.loot.Count != 0) {
                foreach (var item in this.loot) {
                    item.look();
                }
            }

            if (this.skeleton != null && this.key == null && this.loot.Count == 0) {
                this.skeleton.look();
            }
            return true;
        }
        return false;
    }

    /* Calls interact methods for existing entities within this location
        Passed a player class variable to pass to the entity interact methods 
    */
    public virtual bool location_interact(Player player) { 
        if (this.key != null || this.loot.Count != 0 || this.skeleton != null) {
            if (this.key != null) {
                this.key.interact(player);
                this.key = null;
            }
            if (this.loot.Count != 0) {
                foreach (var item in this.loot) {
                    item.interact(player);
                    item.looted = true; 
                }
            }
            if (this.skeleton != null) {
                this.skeleton.interact(player);
            }
        } 
        return true; 
    }

}

/*  ExitLocation Location
    Represents the gate to leave the graveyard
    The location_interact() method will print different prompts depending on whether the player has a key or not
    Contains coords getter and setter in order to track the specific coordinates of the exit location in the level 
*/
class ExitLocation : Location {
    public Coords coords { get; set; }
    
    public ExitLocation(int x, int y){
        this.coords = new Coords(x, y); 
    }

    public override bool location_look() {
        Console.WriteLine("That looks like the gate out of this spooky place!");
        return true;
    }

    public override bool location_interact(Player player) {
        if (player.has_key) {
            Console.WriteLine($"You open the gate with your key!");
            return true;
        } else {
            Console.WriteLine($"You try to open the gate, but it's locked. Must need a key...");
        }
        return false;
    }
}

/* Abstract Entity class used for all entities */
abstract class Entity {
    public virtual  void look() {}
    public abstract void interact(Player player);
}

/*  Key Entity
    Implements the look() and interact() methods
    If interact is called, meaning the player picked up they key, then it will set the 
    player.has_key value to true 
    Look() will simply print the prompt if a key is at the specified location
*/
class Key : Entity {
    public override void look() {
        Console.WriteLine("You see a key on the ground! Might need that to get out of here...");
        
    }

    public override void interact(Player player){
        player.has_key = true;
        Console.WriteLine("You picked up a key!");
    }
}

/*  Loot Entity 
    Implements the look() and interact() methods
    Holds values for the number of coins within the chest and whether it has been looted or not 
        If it has been looted, then the player will not be able to loot it again
    Look() prints prompts dependent on whether the chest has been looted or not
    Interact() Will also print prompts dependent on if it has been looted, as well as adding the coins to the player's coin count
*/
class Loot : Entity {
    public int coins    { get; set; }
    public bool looted  { get; set; }

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
            player.number_of_coins += this.coins;
            Console.WriteLine("You open the chest and find " + this.coins + " coins!");
        }
        else {
            Console.WriteLine($"The chest is empty...");
        }
    }
}

/*  Skeleton Entity
    Implements the look() and interact() methods
    Both methods print their respective prompts 
    Interact method will kill the player
*/
class Skeleton : Entity {
    public override void look() {
        Console.WriteLine("Not much to see here.");
    }

    public override void interact(Player player){
        player.is_dead = true; 
        Console.WriteLine("A bony arm juts out of the ground and grabs your ankle!\nYou've been dragged six feet under by a skeleton.");
    }
}

/*  Player class holds all information relating the player 
    Includes if they are dead, have a key, and the number of coins they have
    Also contains the current player locations and their coords
    Contains methods to see where they are and to print stats 
*/
class Player {
    public Coords coords        { get; set; }
    public Location location    { get; set; }
    public bool is_dead         { get; set; }
    public bool has_key         { get; set; }
    public int number_of_coins  { get; set; }

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

/*  Game
    Loads in the file input for the level & allows for user input
    Contains variables for the number of turns so far, player class, level class, and ExitLocation class
*/
class Game {
    int    num_turns;
    public Player player { get; }
    Level level;
    Location ExitLocation;

    /* Creates the player & initialize all player info */
    public Game() {
        this.player = new Player();
        this.player.is_dead = false; 
        this.player.has_key = false; 
        this.player.number_of_coins = 0;
    }

    /*  Load will read in all file information regarding the level 
        It will create the Level & all unique locations, including the exit and all entities at all locations 
    */
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

                /*  Reads the file and initializes the level. 
                    Starts by creating the level grid of locations (2d arr)
                    Creates the exit location and sets it within the 2d arr
                    Will then create various entities (key, loot, skeleton) for each specified location 
                    All of these locations are accessed via the level 2d arr (once it is created that is)
                */
                switch (split[0]) {
                    case "size":
                        this.level = new Level(x, y);
                        break;

                    case "exit":
                        this.ExitLocation = new ExitLocation(x, y);
                        this.level.arr[x,y] = this.ExitLocation;
                        break;

                    case "key":                   
                        this.level.arr[x,y].key = new Key();
                        break;

                    case "loot":
                        Loot loot = new Loot();
                        loot.coins = count;
                        loot.looted = false;
                        this.level.arr[x,y].loot.Add(loot);
                        break;

                    case "skeleton":
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

    /* Input will take in player input & move them around the level */
    public void input(string line) {
        this.num_turns += 1;

        /* Check for exhaustion (Check if num_turns == 2*w*h) */
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

        switch (split[0]) {

            /* If the player wants to go to a new location */
            case "go":
                /* Check the bounds of the new coordinates -- if out of bounds, break */
                if (this.level.out_of_bounds(new_coords.x, new_coords.y)) break;

                /* Set players new coords and their location */
                this.player.coords = new_coords;
                this.player.location = this.level.arr[new_coords.x, new_coords.y];

                /* If at the exit, determine if the game is over (if player has key) */
                /* Otherwise, call location_look and location_interact at the current location -- prints entities messages) */
                /* If look does not print anything (i.e. no entity there), print "Not much to see here" */
                if (this.level.arr[new_coords.x, new_coords.y] is ExitLocation) {
                    this.level.arr[new_coords.x, new_coords.y].location_look();
                    if (this.level.arr[new_coords.x, new_coords.y].location_interact(this.player)) {
                        this.exit_if_over();
                    } else break;                                  
                }

                if (!this.level.arr[new_coords.x, new_coords.y].location_look()) { Console.WriteLine($"Not much to see here."); }
                this.level.arr[new_coords.x, new_coords.y].location_interact(this.player);
                break;

            /* If the player wants to look at a new location */
            case "look":
                /* Check the bounds of the new coordinates -- if out of bounds, break */
                if (this.level.out_of_bounds(new_coords.x, new_coords.y)) break;

                /* If an exit location, call it's location_look to print prompt */
                /* Otherwise, print the prompts for the entities if they exist */
                /* If look does not print anything (i.e. no entity there), print "Not much to see here" */
                if (this.level.arr[new_coords.x, new_coords.y] is ExitLocation) {
                    this.level.arr[new_coords.x, new_coords.y].location_look();
                    break;
                }
                
                if (!this.level.arr[new_coords.x, new_coords.y].location_look()) { Console.WriteLine($"Not much to see here."); }
                break;
            
            default:
                Console.WriteLine($"Bad command in input: '{line}'");
                return;
        }
    }

    /* Checks the following to determine if the game is over 
        If the player reached the exit with a key
        If the player has made too many moves and has died from exhaustion
        If the player encountered a skeleton
    */
    bool is_over() {
        if (this.player.has_key && this.player.location is ExitLocation) return true;
  
        else if (this.num_turns > (2*this.level.x*this.level.y)) return true;

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