# C# Coding Problems & Solutions

---

## Enum Example

```csharp
enum DaysOfWeek { Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday }

DaysOfWeek today = DaysOfWeek.Monday;
Console.WriteLine("Today is: " + today);
int dayNumber = (int)today;  // Numeric value
Console.WriteLine("Numeric value: " + dayNumber);
```

---

## Find Duplicate Elements in Array

### Using HashSet

```csharp
static List<int> FindDuplicates(int[] arr) {
  var duplicates = new List<int>();
  var seen = new HashSet<int>();

  foreach (var num in arr) {
    if (seen.Contains(num))
      duplicates.Add(num);
    else
      seen.Add(num);
  }
  return duplicates;
}
```

### Using Dictionary

```csharp
static List<int> FindDuplicates(int[] arr) {
  var duplicates = new List<int>();
  var elementCount = new Dictionary<int, int>();

  foreach (var num in arr) {
    if (elementCount.ContainsKey(num)) elementCount[num]++;
    else elementCount[num] = 1;
  }

  foreach (var kvp in elementCount)
    if (kvp.Value > 1) duplicates.Add(kvp.Key);

  return duplicates;
}
```

---

## HttpClient — Call Another API

```csharp
using (HttpClient client = new HttpClient()) {
  client.BaseAddress = new Uri("https://api.example.com/");
  client.DefaultRequestHeaders.Accept.Add(
    new MediaTypeWithQualityHeaderValue("application/json"));

  HttpResponseMessage response = await client.GetAsync("endpoint");
  if (response.IsSuccessStatusCode) {
    string data = await response.Content.ReadAsStringAsync();
    Console.WriteLine(data);
  }
}
```

---

## CRUD Operations via HttpClient

```csharp
public class ProductService {
  private readonly HttpClient _httpClient;

  public async Task<IEnumerable<Product>> GetProductsAsync() =>
    await _httpClient.GetFromJsonAsync<IEnumerable<Product>>("api/products");

  public async Task<Product> CreateProductAsync(Product product) {
    var response = await _httpClient.PostAsJsonAsync("api/products", product);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<Product>();
  }

  public async Task UpdateProductAsync(int id, Product product) {
    var response = await _httpClient.PutAsJsonAsync($"api/products/{id}", product);
    response.EnsureSuccessStatusCode();
  }

  public async Task DeleteProductAsync(int id) {
    var response = await _httpClient.DeleteAsync($"api/products/{id}");
    response.EnsureSuccessStatusCode();
  }
}
```

---

## CRUD API Controller

```csharp
[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase {
  private static List<Product> products = new List<Product> {
    new Product { Id = 1, Name = "Product1", Price = 10.0M }
  };

  [HttpGet] public ActionResult<IEnumerable<Product>> GetProducts() => products;

  [HttpGet("{id}")]
  public ActionResult<Product> GetProduct(int id) {
    var product = products.FirstOrDefault(p => p.Id == id);
    return product == null ? NotFound() : product;
  }

  [HttpPost]
  public ActionResult<Product> PostProduct(Product product) {
    product.Id = products.Max(p => p.Id) + 1;
    products.Add(product);
    return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
  }

  [HttpPut("{id}")]
  public IActionResult PutProduct(int id, Product product) {
    var existing = products.FirstOrDefault(p => p.Id == id);
    if (existing == null) return NotFound();
    existing.Name = product.Name;
    existing.Price = product.Price;
    return NoContent();
  }

  [HttpDelete("{id}")]
  public IActionResult DeleteProduct(int id) {
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product == null) return NotFound();
    products.Remove(product);
    return NoContent();
  }
}
```

---

## Convert String to Char Array

```csharp
string str = "Hello, World!";
char[] charArray = str.ToCharArray();
```

---

## Convert Char Array to String

```csharp
char[] charArray = { 'h', 'e', 'l', 'l', 'o' };
string result = new string(charArray);
```

---

## Happy Number

A number is happy if the sum of squares of its digits eventually reaches 1.

```
19 → 1²+9² = 82 → 8²+2² = 68 → 6²+8² = 100 → 1²+0²+0² = 1 ✓
```

```csharp
public static bool IsHappy(int n) {
  var seen = new HashSet<int>();
  while (n != 1 && !seen.Contains(n)) {
    seen.Add(n);
    n = GetNextNumber(n);
  }
  return n == 1;
}

private static int GetNextNumber(int n) {
  int sum = 0;
  while (n > 0) {
    int digit = n % 10;
    sum += digit * digit;
    n /= 10;
  }
  return sum;
}
```

---

## Bubble Sort

```csharp
public static void Sort(int[] array) {
  int n = array.Length;
  for (int i = 0; i < n - 1; i++)
    for (int j = 0; j < n - i - 1; j++)
      if (array[j] > array[j + 1]) {
        int temp = array[j];
        array[j] = array[j + 1];
        array[j + 1] = temp;
      }
}
```

---

## Top 3 Distinct Numbers

```csharp
public static List<int> GetMaxThreeDistinctNumbers(int[] numbers) {
  return new HashSet<int>(numbers)
    .OrderByDescending(n => n)
    .Take(3)
    .ToList();
}
```

---

## Lambda on Dictionary

### Filtering
```csharp
var expensive = productList.Where(p => p.Value > 2000);
```

### Transforming
```csharp
var discounted = productList.ToDictionary(p => p.Key, p => (int)(p.Value * 0.9));
```

### Finding Max
```csharp
var mostExpensive = productList.OrderByDescending(p => p.Value).FirstOrDefault();
```

---

## Climbing Stairs

You can climb 1 or 2 steps at a time. How many distinct ways to reach step `n`?

```csharp
public static int ClimbStairs(int n) {
  if (n <= 2) return n;
  int[] dp = new int[n + 1];
  dp[1] = 1; dp[2] = 2;
  for (int i = 3; i <= n; i++)
    dp[i] = dp[i - 1] + dp[i - 2];
  return dp[n];
}
```

**Time Complexity:** O(n)

---

## Isomorphic Strings

Two strings are isomorphic if characters in `s` can be replaced to get `t` (no two characters map to the same character).

```csharp
public static bool AreIsomorphic(string s, string t) {
  if (s.Length != t.Length) return false;
  var mapST = new Dictionary<char, char>();
  var mapTS = new Dictionary<char, char>();

  for (int i = 0; i < s.Length; i++) {
    char cs = s[i], ct = t[i];
    if (mapST.ContainsKey(cs) && mapST[cs] != ct) return false;
    if (mapTS.ContainsKey(ct) && mapTS[ct] != cs) return false;
    mapST[cs] = ct;
    mapTS[ct] = cs;
  }
  return true;
}
// "egg" + "add" → true, "foo" + "bar" → false
```

---

## Scrambled Word Finder

Find the word from a list whose letters are all present (and available) in a given note string.

```csharp
public static string Find(List<string> words, string note) {
  var noteCount = GetCharCount(note);
  foreach (var word in words) {
    var wordCount = GetCharCount(word);
    if (wordCount.All(kv => noteCount.ContainsKey(kv.Key) && noteCount[kv.Key] >= kv.Value))
      return word;
  }
  return "-";
}

private static Dictionary<char, int> GetCharCount(string str) {
  var count = new Dictionary<char, int>();
  foreach (var ch in str) {
    if (count.ContainsKey(ch)) count[ch]++;
    else count[ch] = 1;
  }
  return count;
}
```

---

## XML to JSON Conversion

```csharp
using Newtonsoft.Json;

public static string ConvertXmlToJson(string xml) {
  XmlDocument doc = new XmlDocument();
  doc.LoadXml(xml);
  return JsonConvert.SerializeXmlNode(doc);
}
```

---

## Snake Board — Find Passable Rows and Columns

Find all rows and columns entirely filled with `'0'` (no `'+'`).

```csharp
public static (List<int> rows, List<int> cols) FindPassable(char[,] board) {
  int rows = board.GetLength(0), cols = board.GetLength(1);
  var passableRows = new List<int>();
  var passableCols = new List<int>();

  for (int i = 0; i < rows; i++) {
    bool ok = true;
    for (int j = 0; j < cols; j++) if (board[i, j] == '+') { ok = false; break; }
    if (ok) passableRows.Add(i);
  }

  for (int j = 0; j < cols; j++) {
    bool ok = true;
    for (int i = 0; i < rows; i++) if (board[i, j] == '+') { ok = false; break; }
    if (ok) passableCols.Add(j);
  }

  return (passableRows, passableCols);
}
```

**Time Complexity:** O(n × m)

---

## Mahjong — Complete Hand Check

A complete hand has groups of triples and **exactly one pair**.

```csharp
public static bool IsCompleteHand(string tiles) {
  var count = new Dictionary<char, int>();
  foreach (var tile in tiles) {
    if (count.ContainsKey(tile)) count[tile]++;
    else count[tile] = 1;
  }

  bool hasPair = false;
  foreach (var c in count.Values) {
    if (c % 3 == 1) return false;       // remainder 1 → impossible
    if (c % 3 == 2) {
      if (hasPair) return false;        // only one pair allowed
      hasPair = true;
    }
  }
  return hasPair;
}
// "88844" → true, "111333555" → false (no pair), "99" → true
```

**Time Complexity:** O(n)

---

## Sub-Sudoku Validator

Returns `true` if every row and column contains the numbers `1..N` exactly once.

```csharp
public static bool IsValidSubSudoku(int[][] grid) {
  int n = grid.Length;
  for (int i = 0; i < n; i++)
    if (!IsValidSet(grid[i], n)) return false;

  for (int j = 0; j < n; j++) {
    int[] column = new int[n];
    for (int i = 0; i < n; i++) column[i] = grid[i][j];
    if (!IsValidSet(column, n)) return false;
  }
  return true;
}

private static bool IsValidSet(int[] set, int n) {
  bool[] seen = new bool[n + 1];
  foreach (int num in set) {
    if (num < 1 || num > n || seen[num]) return false;
    seen[num] = true;
  }
  return true;
}
```

**Time Complexity:** O(N²)

---

## Robot Factory — Assemble Complete Robots

Find robots for which all required parts are available.

```csharp
public static List<string> GetRobots(List<string> allParts, string requiredParts) {
  var required = new HashSet<string>(requiredParts.Split(','));
  var robotParts = new Dictionary<string, HashSet<string>>();

  foreach (var part in allParts) {
    var split = part.Split('_');
    if (!robotParts.ContainsKey(split[0])) robotParts[split[0]] = new HashSet<string>();
    robotParts[split[0]].Add(split[1]);
  }

  return robotParts
    .Where(r => required.All(p => r.Value.Contains(p)))
    .Select(r => r.Key)
    .ToList();
}
// required = "sensors,case,speaker,wheels" → ["Bolt", "Rocket"]
```

**Time Complexity:** O(N × P)

---

## Anagram Checker

```csharp
public static bool AreAnagrams(string str1, string str2) {
  if (str1.Length != str2.Length) return false;
  char[] a = str1.ToCharArray(), b = str2.ToCharArray();
  Array.Sort(a); Array.Sort(b);
  return a.SequenceEqual(b);
}
// "hello" and "elloh" → true
```
