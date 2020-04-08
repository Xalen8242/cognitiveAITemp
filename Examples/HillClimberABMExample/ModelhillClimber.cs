using hillClimber;

// todo make this part of our library also?
public static class Program {

	// todo get these from somewhere else automatically
	public static string OUTPUT_FILENAME = "Animal.csv";
	public static string FITNESS_COLUMNNAME = "BioEnergy";
	public static int STEPS = 10;
	public static void Main(string[] args) {

		int epochs = 10;

		HillClimberFCM fcm = new HillClimberFCM(6, 120, STEPS, OUTPUT_FILENAME, FITNESS_COLUMNNAME);

		// loop that runs through the MARS simulations
		for (int i = 0; i < epochs; i++)
		{

			// before the MARS sim we need to initialize the agent init values
			// for MARS to load them we have to put them into an agent init file names animal_init.csv

			if (args != null && System.Linq.Enumerable.Any(args, s => s.Equals("-l")))
			{
				Mars.Common.Logging.LoggerFactory.SetLogLevel(Mars.Common.Logging.Enums.LogLevel.Info);
				Mars.Common.Logging.LoggerFactory.ActivateConsoleLogging();
			}

			var description = new Mars.Core.ModelContainer.Entities.ModelDescription();
			description.AddLayer<Terrain>();
			description.AddAgent<Animal, Terrain>();

			var task = Mars.Core.SimulationStarter.SimulationStarter.Start(description, args);

			
			var loopResults = task.Run();

			System.Console.WriteLine($"Simulation execution finished after {loopResults.Iterations} steps");

			// run the FCM which updates the genome values
			fcm.Run();
		}
	}
}
