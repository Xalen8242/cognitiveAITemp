namespace hillClimber
{
	// Pragma and ReSharper disable all warnings for generated code
	#pragma warning disable 162
	#pragma warning disable 219
	#pragma warning disable 169
    
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Mars.Common.Logging;
    using Mars.Components.Environments;
    using Mars.Interfaces.Environment;
    using Mars.Interfaces.Layer;
    using Mars.Mathematics;

    public class Terrain : Mars.Components.Layers.AbstractLayer
    {
        private static readonly ILogger _Logger = Mars.Common.Logging.LoggerFactory.GetLogger(typeof(Terrain));

        private readonly Random random = new Random();

        public UnregisterAgent _Unregister { get; set; }

        public RegisterAgent _Register { get; set; }

        public SpaceDistanceMetric _DistanceMetric { get; set; } = Mars.Mathematics.SpaceDistanceMetric.Euclidean;

        private int _dimensionX, _dimensionY;

        public int DimensionX() => _dimensionX;
        public int DimensionY() => _dimensionY;

        public ConcurrentDictionary<Position, string> StringTerrain { get; set; }

        public ConcurrentDictionary<Position, double> RealTerrain { get; set; }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public string GetStringValue(int x, int y) => GetStringValue((double)x, y);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public string GetStringValue(double x, double y) =>
            StringTerrain.TryGetValue(Mars.Interfaces.Environment.Position.CreatePosition(x, y), out var value) ? value : null;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int GetIntegerValue(int x, int y) => GetIntegerValue((double)x, y);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int GetIntegerValue(double x, double y) =>
                    RealTerrain.TryGetValue(Mars.Interfaces.Environment.Position.CreatePosition(x, y), out var value) ? Convert.ToInt32(value) : 0;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public double GetRealValue(int x, int y) => GetRealValue((double)x, y);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public double GetRealValue(double x, double y) =>
            RealTerrain.TryGetValue(Mars.Interfaces.Environment.Position.CreatePosition(x, y), out var value) ? value : 0;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetStringValue(int x, int y, string value) => SetStringValue((double)x, y, value);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetStringValue(double x, double y, string value)
        {
            if (value != null)
            {
                StringTerrain.AddOrUpdate(Position.CreatePosition(x, y), value, (position, s) => value);
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetIntegerValue(double x, double y, int value) => SetRealValue(x, y, value);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetIntegerValue(int x, int y, int value) => SetRealValue((double)x, y, value);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetRealValue(int x, int y, double value) => SetRealValue((double)x, y, value);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetRealValue(double x, double y, double value)
        {
            if (Math.Abs(value) > 0.000000001)
            {
                RealTerrain.AddOrUpdate(Position.CreatePosition(x, y), value, (position, s) => value);
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void _InitGrid(Mars.Interfaces.Layer.Initialization.TInitData initData)
        {
            if (initData.LayerInitConfig != null && !string.IsNullOrEmpty(initData.LayerInitConfig.File))
            {
                var table = Mars.Common.IO.Csv.CsvReader.MapData(initData.LayerInitConfig.File, null, false);

                var xMaxIndex = table.Columns.Count;
                int yMaxIndex = table.Rows.Count - 1;

                _dimensionX = table.Columns.Count;
                _dimensionY = table.Rows.Count;
                foreach (System.Data.DataRow tableRow in table.Rows)
                {
                    for (int x = 0; x < xMaxIndex; x++)
                    {
                        var value = tableRow[x].ToString();
                        if (double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture,
                            out var result))
                            SetRealValue(x, yMaxIndex, result);
                        else
                            SetStringValue(x, yMaxIndex, value);
                    }
                    yMaxIndex--;
                }
            }
        }

        public SpatialHashEnvironment<Animal> _AnimalEnvironment { get; set; }
        public IDictionary<System.Guid, Animal> _AnimalAgents { get; set; }

        public Terrain _Terrain => this;
        public Terrain terrain => this;
        public Terrain(double dimensionX = 100, double dimensionY = 100, int cellSize = 1)
        {
            _dimensionX = Convert.ToInt32(dimensionX); _dimensionY = Convert.ToInt32(dimensionY);
            StringTerrain = new ConcurrentDictionary<Position, string>();
            RealTerrain = new ConcurrentDictionary<Position, double>();
        }

        public override bool InitLayer(
            Mars.Interfaces.Layer.Initialization.TInitData initData,
            RegisterAgent regHandle,
            UnregisterAgent unregHandle)
        {
            base.InitLayer(initData, regHandle, unregHandle);
            this._Register = regHandle;
            this._Unregister = unregHandle;

            _InitGrid(initData);
            this._AnimalEnvironment = new SpatialHashEnvironment<Animal>(_dimensionX, _dimensionY, true);

            _AnimalAgents = Mars.Components.Services.AgentManager.SpawnAgents<Animal>(
            initData.AgentInitConfigs.First(config => config.Type == typeof(Animal)),
            regHandle, unregHandle,
            new List<ILayer> { this });

            return true;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public hillClimber.Animal _SpawnAnimal(double xcor = 0, double ycor = 0, int freq = 1)
        {
            var id = System.Guid.NewGuid();
            var agent = new hillClimber.Animal(
				id, 
				this, 
				_Register,
				_Unregister,
            	_AnimalEnvironment,
            	default(int),
            	default(int),
                default(int),
				xcor, 
				ycor, 
				freq);

            _AnimalAgents.Add(id, agent);
            return agent;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void KillAnimal(hillClimber.Animal target, int executionFrequency = 1)
        {
            target.isAlive = false;
            _AnimalEnvironment.Remove(target);
            _Unregister(this, target, target.executionFrequency);
            _AnimalAgents.Remove(target.ID);
        }
    }
}
