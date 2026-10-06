using System;

public partial class Form1
{
    private RunSequence normalSequence;
    private RunSequence rushSequence;

    private void InitializeRunSequences()
    {
        normalSequence = new RunSequence(
            new RunStep(() => CharConfig.RunWPTaker && !WPTaker_0.ScriptDone, () => WPTaker_0.RunScript()),
            new RunStep(() => CharConfig.RunShopBotScript && !ShopBot_0.ScriptDone, () => ShopBot_0.RunScript()),
            new RunStep(() => CharConfig.RunMausoleumScript && !Mausoleum_0.ScriptDone, () => Mausoleum_0.RunScript()),
            new RunStep(() => CharConfig.RunCryptScript && !Crypt_0.ScriptDone, () => Crypt_0.RunScript()),
            new RunStep(() => CharConfig.RunPitScript && !Pit_0.ScriptDone, () => Pit_0.RunScript()),
            new RunStep(() => CharConfig.RunCowsScript && !Cows_0.ScriptDone, () => Cows_0.RunScript()),
            new RunStep(() => CharConfig.RunCountessScript && !Countess_0.ScriptDone, () => Countess_0.RunScript()),
            new RunStep(() => CharConfig.RunAndarielScript && !Andariel_0.ScriptDone, () => Andariel_0.RunScript()),
            new RunStep(() => CharConfig.RunSummonerScript && !Summoner_0.ScriptDone, () => Summoner_0.RunScript()),
            new RunStep(() => CharConfig.RunDurielScript && !Duriel_0.ScriptDone, () => Duriel_0.RunScript()),
            new RunStep(() => CharConfig.RunArachnidScript && !ArachnidLair_0.ScriptDone, () => ArachnidLair_0.RunScript()),
            new RunStep(() => CharConfig.RunLowerKurastScript && !LowerKurast_0.ScriptDone, () => LowerKurast_0.RunScript()),
            new RunStep(() => CharConfig.RunA3SewersScript && !Act3Sewers_0.ScriptDone, () => Act3Sewers_0.RunScript()),
            new RunStep(() => CharConfig.RunUpperKurastScript && !UpperKurast_0.ScriptDone, () => UpperKurast_0.RunScript()),
            new RunStep(() => CharConfig.RunTravincalScript && !Travincal_0.ScriptDone, () => Travincal_0.RunScript()),
            new RunStep(() => CharConfig.RunMephistoScript && !Mephisto_0.ScriptDone, () => Mephisto_0.RunScript()),
            new RunStep(() => CharConfig.RunChaosScript && !Chaos_0.ScriptDone, () => Chaos_0.RunScript()),
            new RunStep(() => CharConfig.RunChaosLeechScript && !ChaosLeech_0.ScriptDone, () => ChaosLeech_0.RunScript()),
            new RunStep(() => CharConfig.RunEldritchScript && !Eldritch_0.ScriptDone, () => Eldritch_0.RunScript()),
            new RunStep(() => CharConfig.RunShenkScript && !Shenk_0.ScriptDone, () => Shenk_0.RunScript()),
            new RunStep(() => CharConfig.RunFrozensteinScript && !Frozenstein_0.ScriptDone, () => Frozenstein_0.RunScript()),
            new RunStep(() => CharConfig.RunPindleskinScript && !Pindleskin_0.ScriptDone, () => Pindleskin_0.RunScript()),
            new RunStep(() => CharConfig.RunNihlatakScript && !Nihlatak_0.ScriptDone, () => Nihlatak_0.RunScript()),
            new RunStep(() => CharConfig.RunBaalScript && !Baal_0.ScriptDone, () => Baal_0.RunScript()),
            new RunStep(() => CharConfig.RunBaalLeechScript && !BaalLeech_0.ScriptDone, () => BaalLeech_0.RunScript()),
            new RunStep(() => CharConfig.RunTerrorZonesScript && !TerrorZones_0.ScriptDone, () => TerrorZones_0.RunScript()));

        rushSequence = new RunSequence(
            new RunStep(() => CharConfig.RunDarkWoodRush && !DarkWoodRush_0.ScriptDone, () => DarkWoodRush_0.RunScript()),
            new RunStep(() => CharConfig.RunTristramRush && !TristramRush_0.ScriptDone, () => TristramRush_0.RunScript()),
            new RunStep(() => CharConfig.RunAndarielRush && !AndarielRush_0.ScriptDone, () => AndarielRush_0.RunScript()),
            new RunStep(() => CharConfig.RunRadamentRush && !RadamentRush_0.ScriptDone, () => RadamentRush_0.RunScript()),
            new RunStep(() => CharConfig.RunHallOfDeadRush && !HallOfDeadRushCube_0.ScriptDone, () => HallOfDeadRushCube_0.RunScript()),
            new RunStep(() => CharConfig.RunFarOasisRush && !FarOasisRush_0.ScriptDone, () => FarOasisRush_0.RunScript()),
            new RunStep(() => CharConfig.RunLostCityRush && !LostCityRush_0.ScriptDone, () => LostCityRush_0.RunScript()),
            new RunStep(() => CharConfig.RunSummonerRush && !SummonerRush_0.ScriptDone, () => SummonerRush_0.RunScript()),
            new RunStep(() => CharConfig.RunDurielRush && !DurielRush_0.ScriptDone, () => DurielRush_0.RunScript()),
            new RunStep(() => CharConfig.RunKahlimEyeRush && !KahlimEyeRush_0.ScriptDone, () => KahlimEyeRush_0.RunScript()),
            new RunStep(() => CharConfig.RunKahlimBrainRush && !KahlimBrainRush_0.ScriptDone, () => KahlimBrainRush_0.RunScript()),
            new RunStep(() => CharConfig.RunKahlimHeartRush && !KahlimHeartRush_0.ScriptDone, () => KahlimHeartRush_0.RunScript()),
            new RunStep(() => CharConfig.RunTravincalRush && !TravincalRush_0.ScriptDone, () => TravincalRush_0.RunScript()),
            new RunStep(() => CharConfig.RunMephistoRush && !MephistoRush_0.ScriptDone, () => MephistoRush_0.RunScript()),
            new RunStep(() => CharConfig.RunChaosRush && !ChaosRush_0.ScriptDone, () => ChaosRush_0.RunScript()),
            new RunStep(() => CharConfig.RunAnyaRush && !AnyaRush_0.ScriptDone, () => AnyaRush_0.RunScript()),
            new RunStep(() => CharConfig.RunAncientsRush && !AncientsRush_0.ScriptDone, () => AncientsRush_0.RunScript()),
            new RunStep(() => CharConfig.RunBaalRush && !BaalRush_0.ScriptDone, () => BaalRush_0.RunScript()));

    }
}
