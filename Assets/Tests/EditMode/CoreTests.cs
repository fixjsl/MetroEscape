using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Core 어셈블리(게임과 무관한 바닥 코드) 테스트

public class StatModifierSetTests
{
    [Test]
    public void 배율이_없으면_1()
    {
        var set = new StatModifierSet();
        Assert.AreEqual(1f, set.GetMultiplier(StatType.Stamina));
    }

    [Test]
    public void 올리는_배율은_가장_큰_것만_적용()
    {
        var set = new StatModifierSet();
        set.Add(new StatModifier(StatType.Stamina, 1.5f, this));
        set.Add(new StatModifier(StatType.Stamina, 2f, this));
        Assert.AreEqual(2f, set.GetMultiplier(StatType.Stamina), 1e-4f);
    }

    [Test]
    public void 올리는_것과_내리는_것은_곱해짐()
    {
        var set = new StatModifierSet();
        set.Add(new StatModifier(StatType.Speed, 2f, this));
        set.Add(new StatModifier(StatType.Speed, 0.5f, this));
        Assert.AreEqual(1f, set.GetMultiplier(StatType.Speed), 1e-4f);
    }

    [Test]
    public void 다른_스탯의_배율은_영향_없음()
    {
        var set = new StatModifierSet();
        set.Add(new StatModifier(StatType.Hungry, 3f, this));
        Assert.AreEqual(1f, set.GetMultiplier(StatType.Thirst));
    }

    [Test]
    public void Remove하면_원래대로()
    {
        var set = new StatModifierSet();
        var m = new StatModifier(StatType.Stamina, 2f, this);
        set.Add(m);
        set.Remove(m);
        Assert.AreEqual(1f, set.GetMultiplier(StatType.Stamina));
    }
}

public class EffectRunnerTests
{
    // 효과를 받는 가짜 대상
    class FakeTarget : IEffectTarget, IDamageable
    {
        public StatModifierSet Modifiers { get; } = new StatModifierSet();
        public float hp;
        public void Heal(float amount) => hp += amount;
        public void TakeDamage(float amount) => hp -= amount;
    }

    [Test]
    public void 배율_효과는_걸린_동안만_적용되고_끝나면_빠짐()
    {
        var target = new FakeTarget();
        var runner = new EffectRunner(target);
        var effect = new StatMultiplierEffect { stat = StatType.Stamina, multiplier = 2f, duration = 3f };

        runner.Apply(effect, this);
        Assert.AreEqual(2f, target.Modifiers.GetMultiplier(StatType.Stamina), 1e-4f);

        runner.Tick(2f);
        Assert.AreEqual(1, runner.ActiveEffects.Count);

        runner.Tick(1.5f);
        Assert.AreEqual(0, runner.ActiveEffects.Count);
        Assert.AreEqual(1f, target.Modifiers.GetMultiplier(StatType.Stamina), 1e-4f);
    }

    [Test]
    public void 같은_효과를_다시_걸면_시간이_초기화됨()
    {
        var runner = new EffectRunner(new FakeTarget());
        var effect = new StatMultiplierEffect { stat = StatType.Speed, multiplier = 1.5f, duration = 5f };

        runner.Apply(effect, this);
        runner.Tick(4f);
        runner.Apply(effect, this);

        Assert.AreEqual(1, runner.ActiveEffects.Count);
        Assert.AreEqual(5f, runner.ActiveEffects[0].remaining, 1e-4f);
    }

    [Test]
    public void 회복_효과는_틱마다_회복하고_다시_걸면_시간이_늘어남()
    {
        var target = new FakeTarget();
        var runner = new EffectRunner(target);
        var heal = new HealOverTimeEffect { healPerSecond = 2f, duration = 10f };

        runner.Apply(heal, this);
        runner.Tick(1f);
        Assert.AreEqual(2f, target.hp, 1e-4f);

        runner.Apply(heal, this);                       // 재사용 → 시간 연장
        Assert.AreEqual(19f, runner.ActiveEffects[0].remaining, 1e-4f);
    }

    [Test]
    public void 무한_효과는_시간이_지나도_안_끝남()
    {
        var runner = new EffectRunner(new FakeTarget());
        runner.Apply(new StatMultiplierEffect { multiplier = 2f, duration = 0f }, this);
        runner.Tick(1000f);
        Assert.AreEqual(1, runner.ActiveEffects.Count);
    }

    [Test]
    public void ClearOnDeath는_죽으면_지우는_효과만_지움()
    {
        var runner = new EffectRunner(new FakeTarget());
        runner.Apply(new StatMultiplierEffect { duration = 10f, removeOnDeath = true }, this);
        runner.Apply(new StatMultiplierEffect { duration = 10f, removeOnDeath = false }, this);
        runner.ClearOnDeath();
        Assert.AreEqual(1, runner.ActiveEffects.Count);
        Assert.IsFalse(runner.ActiveEffects[0].def.removeOnDeath);
    }
}

public class StateMachineTests
{
    static readonly List<string> calls = new List<string>();

    abstract class TestState : State
    {
        public override void Enter() => calls.Add(GetType().Name + ".Enter");
        public override void Exit() => calls.Add(GetType().Name + ".Exit");
    }
    class A : TestState { }
    class B : TestState { }

    [SetUp] public void Clear() => calls.Clear();

    [Test]
    public void 상태를_바꾸면_Exit_다음에_Enter()
    {
        var fsm = new StateMachine<TestState>();
        fsm.Register(new A());
        fsm.Register(new B());

        fsm.Start<A>();
        fsm.ChangeState<B>();

        CollectionAssert.AreEqual(new[] { "A.Enter", "A.Exit", "B.Enter" }, calls);
        Assert.IsInstanceOf<B>(fsm.Current);
    }

    [Test]
    public void 등록_안_된_상태로는_안_바뀜()
    {
        var fsm = new StateMachine<TestState>();
        fsm.Register(new A());
        fsm.Start<A>();

        LogAssert.Expect(LogType.Error, "Error");      // StateMachine이 남기는 에러 로그
        fsm.ChangeState<B>();

        Assert.IsInstanceOf<A>(fsm.Current);
    }
}

public class IntervalTimerTests
{
    [Test]
    public void 간격이_되면_true_넘친_시간은_다음으로()
    {
        var t = new IntervalTimer();
        Assert.IsFalse(t.Timer(0.1f, 0.06f));
        Assert.IsTrue(t.Timer(0.1f, 0.06f));            // 0.12 → 걸림, 0.02 남음
        Assert.IsFalse(t.Timer(0.1f, 0.06f));           // 0.08
        Assert.IsTrue(t.Timer(0.1f, 0.03f));            // 0.11 → 걸림
    }

    [Test]
    public void Reset하면_처음부터()
    {
        var t = new IntervalTimer();
        t.Timer(1f, 0.9f);
        t.Reset();
        Assert.IsFalse(t.Timer(1f, 0.5f));
    }
}

public class JsonFileStoreTests
{
    [System.Serializable]
    class Data { public int number = 7; public string text = "기본"; }

    string dir;

    [SetUp] public void MakeDir() { dir = Path.Combine(Path.GetTempPath(), "MetroEscapeTests_" + System.Guid.NewGuid().ToString("N")); Directory.CreateDirectory(dir); }
    [TearDown] public void RemoveDir() { if (Directory.Exists(dir)) Directory.Delete(dir, true); }

    [Test]
    public void 저장했다_읽으면_같은_값()
    {
        string path = Path.Combine(dir, "a.json");
        Assert.IsTrue(JsonFileStore.Save(path, new Data { number = 42, text = "저장" }));
        var loaded = JsonFileStore.LoadorDefault<Data>(path);
        Assert.AreEqual(42, loaded.number);
        Assert.AreEqual("저장", loaded.text);
    }

    [Test]
    public void 파일이_없으면_기본값()
    {
        var loaded = JsonFileStore.LoadorDefault<Data>(Path.Combine(dir, "없음.json"));
        Assert.AreEqual(7, loaded.number);
    }

    [Test]
    public void 깨진_파일이면_기본값과_백업()
    {
        string path = Path.Combine(dir, "broken.json");
        File.WriteAllText(path, "{ 이건 JSON 아님");
        LogAssert.ignoreFailingMessages = true;          // 경고 로그는 정상 동작
        var loaded = JsonFileStore.LoadorDefault<Data>(path);
        Assert.AreEqual(7, loaded.number);
        Assert.IsTrue(File.Exists(path + ".bak"));
    }
}
