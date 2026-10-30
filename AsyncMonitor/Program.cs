using AsyncMonitor;
var service = new CheckService();
var cts = new CancellationTokenSource();
var checks = new (string Name, TimeSpan Delay)[]
{
    ("Сеть", TimeSpan.FromMilliseconds(700)),
    ("База данных", TimeSpan.FromMilliseconds(900)),
    ("Файлы", TimeSpan.FromMilliseconds(500))
};
while (true)
{
    Console.WriteLine("\n1 — одна проверка; 2 — последовательно; 3 — параллельно; 0 — выход");
    var cmd = Console.ReadLine();
    if (cmd == "0")
        break;
    try
    {
        if (cmd == "1")
        {
            // TODO 8: Вызвать метод с await, не применять .Wait() или .Result.
            var result = await service.CheckServiceAsync(checks[0].Name, checks[0].Delay, cts.Token);
            Console.WriteLine($"Одна проверка: {result.Name} - {result.Duration.TotalMilliseconds} мс");
        }
        else if (cmd == "2")
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var r = await service.RunSequentialAsync(checks);
            sw.Stop();
            Console.WriteLine($"Последовательно: {sw.ElapsedMilliseconds} мс");
            Print(r);
        }
        else if (cmd == "3")
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var r = await service.RunParallelAsync(checks);
            sw.Stop();
            Console.WriteLine($"Параллельно: {sw.ElapsedMilliseconds} мс");
            Print(r);
        }
        else
            Console.WriteLine("Неизвестная команда.");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Проверка отменена.");
    }
    catch (Exception ex)
    {
        // TODO 9: Не скрывать исключение; сообщить тип и причину.
        throw new Exception($"[Ошибка] Тип исключения: {ex.GetType().Name}, Причина: {ex.Message}");
    }
}
// TODO 10: Добавить режим контроля/отмены длительной проверки и показать,
// что меню не зависает на ожидании Task.Delay.
static void Print(IEnumerable<CheckResult> xs)
{
    static void Print(IEnumerable<CheckResult> xs)
    {
        Console.WriteLine("Результаты проверок:");
        foreach (var x in xs)
        {
            var status = x.Success ? "PASS" : "ERROR";
            Console.WriteLine($"  - {x.Name}: {status} за {x.Duration.TotalMilliseconds} мс. ({x.Message})");
        }
    }
}
