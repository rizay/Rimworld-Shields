using CentralizedClimateControl;

namespace FrontierDevelopments.General.Comps
{
    public class Comp_ClimateControlHeatSink : Comp_HeatSink
    {
        private CompAirFlowConsumer _compAirFlowConsumer;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _compAirFlowConsumer = parent.GetComp<CompAirFlowConsumer>();
        }

        private bool AirConnectedConnected()
        {
            return _compAirFlowConsumer.IsActive();
        }

        protected override double AmbientTemp()
        {
            return AirConnectedConnected() ? _compAirFlowConsumer.AirFlowNet.AverageConvertedTemperature : base.AmbientTemp();
        }
    }
}