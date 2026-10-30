using AsyncMonitor;
var passed = 0;
var failed = 0;
void Check(bool ok, string name)
{
    if (ok)
    {
        passed++;
        Console.WriteLine("PASS " + name);
    }
    else
    {
        failed++;
        Console.WriteLine("FAIL " + name);
    }
}
var s = new CheckService();
var one = await s.CheckServiceAsync("Тест", TimeSpan.FromMilliseconds(25));
Check
    (
    one.Name == "Тест" && one.Success && one.Duration >= TimeSpan.Zero,
    "одиночная проверка возвращает результат"
    );
var seq = await s.RunSequentialAsync
    (
    new[]
    {
        ("A", TimeSpan.FromMilliseconds(60)),
        ("B", TimeSpan.FromMilliseconds(60)),
        ("C", TimeSpan.FromMilliseconds(60))
    }
    );
Check(
    seq.Count == 3 && seq.Select(x => x.Name).SequenceEqual(new[] { "A", "B", "C" }),
    "последовательный запуск"
);
var all = await s.RunParallelAsync
    (
    new[]
    {
        ("A", TimeSpan.FromMilliseconds(60)),
        ("B", TimeSpan.FromMilliseconds(60)),
        ("C", TimeSpan.FromMilliseconds(60))
    }
    );
Check(all.Count == 3 && all.All(x => x.Success), "параллельный запуск WhenAll");
// TODO: Добавить тесты на пустое имя, отрицательную задержку и CancellationToken.

// Тест на пустое имя
bool emptyNameThrows = false;
try
{
    await s.CheckServiceAsync("", TimeSpan.FromMilliseconds(10));
}
catch (ArgumentException)
{
    emptyNameThrows = true;
}
Check(emptyNameThrows, "пустое имя вызывает исключение");

// Тест на отрицательную задержку
bool negativeDelayThrows = false;
try
{
    await s.CheckServiceAsync("Тест", TimeSpan.FromMilliseconds(-10));
}
catch (ArgumentOutOfRangeException)
{
    negativeDelayThrows = true;
}
Check(negativeDelayThrows, "отрицательная задержка вызывает ArgumentOutOfRangeException");

// Тест на CancellationToken
bool cancelThrows = false;
using var cts = new CancellationTokenSource();
cts.Cancel();
try
{
    await s.CheckServiceAsync("Тест", TimeSpan.FromMilliseconds(500), cts.Token);
}
catch (OperationCanceledException)
{
    cancelThrows = true;
}
Check(cancelThrows, "отмена через CancellationToken вызывает OperationCanceledException");

Console.WriteLine($"Итого: {passed} пройдено, {failed} ошибок.");
return failed == 0 ? 0 : 1;
