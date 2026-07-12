# Zomato Data Analyst Interview Questions (0-3 Years) | 5-25 LPA

## Data Analyst Questions

---

### 1. Write a query to find duplicate rows in a table.

To detect duplicates, identify columns that should be unique and group by them.

**Example:**
```sql
SELECT column1, column2, COUNT(*) AS count
FROM your_table
GROUP BY column1, column2
HAVING COUNT(*) > 1;
```

**Explanation:**
- `GROUP BY` combines rows with the same values in the specified columns.
- `HAVING COUNT(*) > 1` filters those combinations that occur more than once, indicating duplicates.

> **Tip:** Add `ROW_NUMBER()` or `RANK()` with CTE to highlight or delete duplicates if needed.

---

### 2. Write a SQL query to calculate the running average of daily orders for each restaurant.

```sql
SELECT restaurant_id,
       order_date,
       AVG(order_count) OVER (
           PARTITION BY restaurant_id
           ORDER BY order_date
           ROWS BETWEEN 6 PRECEDING AND CURRENT ROW
       ) AS seven_day_avg
FROM daily_orders;
```

**Explanation:**
- `PARTITION BY restaurant_id` computes separately for each restaurant.
- `ROWS BETWEEN 6 PRECEDING AND CURRENT ROW` creates a 7-day rolling window.
- `AVG(order_count)` gives the average order count across that window.

---

### 3. Implement a sliding window algorithm to find the maximum sum of any subarray of size k.

```python
def max_subarray_sum(arr, k):
    n = len(arr)
    window_sum = sum(arr[:k])
    max_sum = window_sum

    for i in range(k, n):
        window_sum += arr[i] - arr[i - k]
        max_sum = max(max_sum, window_sum)

    return max_sum

# Example
print(max_subarray_sum([2, 1, 5, 1, 3, 2], 3))  # 9
```

**Tips:**
- Initialize the sum of the first window.
- Slide the window by subtracting the element going out and adding the new one.
- Track the maximum sum seen so far.
- Runs in O(n) instead of O(n*k).

---

### 4. Design an API rate limiter using the Token Bucket algorithm.

**Conceptual Design:**
- Bucket capacity: max tokens allowed (e.g., 1000)
- Refill rate: tokens added per second (e.g., 10/sec)
- Each incoming request consumes 1 token
- If bucket empty → reject request (HTTP 429 Too Many Requests)

**Explanation:**
- Bucket refills at a fixed rate (smooth traffic).
- Prevents sudden spikes by limiting burst size.
- Simple to implement in Redis or in-memory cache.

> **Tip:** Use Redis for distributed systems to share bucket state.

---

### 5. Write Python code to analyze cancellation spikes using pandas.

```python
import pandas as pd

# Sample DataFrame
df = pd.DataFrame({
    'order_id': [1, 2, 3, 4, 5, 6],
    'status': ['delivered', 'cancelled', 'cancelled', 'delivered', 'cancelled', 'cancelled'],
    'reason': ['NA', 'late delivery', 'wrong item', 'NA', 'late delivery', 'late delivery']
})

# Count cancellations by reason
cancellation_analysis = df[df['status'] == 'cancelled'] \
                        .groupby('reason').size().reset_index(name='count')

print(cancellation_analysis)
```

**Explanation:**
- Filter orders with `status = cancelled`.
- Group by reason to find dominant causes.
- Helps explain spikes with data storytelling.

> **Tip:** Join with delivery-time features to show late deliveries vs wrong items trend.

---

### 6. Get top 3 highest-paid employees per department (medium).

```sql
SELECT department, employee, salary
FROM (
    SELECT department, employee, salary,
           ROW_NUMBER() OVER (PARTITION BY department ORDER BY salary DESC) AS rn
    FROM employees
) t
WHERE rn <= 3;
```

**Explanation:**
- `ROW_NUMBER()` ranks employees within each department.
- Filtering with `rn <= 3` gives top 3.

> **Tip:** Use `RANK()` instead of `ROW_NUMBER()` if you want to include ties.

---

### 7. Write a query to fetch the top 3 performing products based on sales.

Assume table `sales_data` has: `product_id`, `product_name`, `total_sales`

```sql
SELECT product_id, product_name, total_sales
FROM sales_data
ORDER BY total_sales DESC
LIMIT 3;
```

**Alternate using RANK() (if ties matter):**
```sql
SELECT product_id, product_name, total_sales
FROM (
    SELECT *, RANK() OVER (ORDER BY total_sales DESC) AS rank_num
    FROM sales_data
) ranked_sales
WHERE rank_num <= 3;
```

---

### 8. Explain the difference between UNION and UNION ALL.

| Feature | UNION | UNION ALL |
|---------|-------|-----------|
| Duplicates | Removes duplicates | Keeps all rows, including duplicates |
| Performance | Slower (because of sorting) | Faster (no de-duplication) |
| Use case | When you want distinct rows | When duplicates are meaningful |

**Example:**
```sql
SELECT city FROM customers
UNION
SELECT city FROM vendors;
-- Returns a unique list of cities.

SELECT city FROM customers
UNION ALL
SELECT city FROM vendors;
-- Returns all cities, including duplicates.
```

---

### 9. Convert categorical variable into dummy variables.

```python
df_encoded = pd.get_dummies(df, columns=['Gender', 'City'], drop_first=True)
```

**Explanation:**
- `get_dummies()` converts categories into binary columns (one-hot encoding).
- `drop_first=True` avoids dummy variable trap (perfect collinearity).

> **Tip:** For ML, prefer `sklearn.preprocessing.OneHotEncoder` for pipeline compatibility.

---

### 10. Explain p-value in hypothesis testing.

The **p-value** is the probability of observing the sample result (or more extreme) if the null hypothesis is true.

**Interpretation:**
- Small p-value (**< 0.05**) → reject null (evidence against H₀).
- Large p-value → fail to reject null.

> **Tip:** p-value ≠ probability that H₀ is true. It measures consistency of data with H₀.

---

### 11. What is a CTE (Common Table Expression), and how is it used?

**Definition:**
A **CTE (Common Table Expression)** is a temporary, named result set that you can reference within a SQL query. It improves readability and simplifies complex subqueries or recursive logic.

**Syntax:**
```sql
WITH cte_name AS (
    SELECT ...
)
SELECT * FROM cte_name;
```

**Example – Filter top-paid employees using CTE:**
```sql
WITH HighEarners AS (
    SELECT emp_id, name, salary
    FROM employees
    WHERE salary > 100000
)
SELECT * FROM HighEarners;
```

**Benefits:**
- Reusable and readable
- Allows recursion (e.g., hierarchical data)
- Avoids repeating subqueries

---

### 12. Write a query to identify customers who have made transactions above $5,000 multiple times.

Assume `transactions` table has: `customer_id`, `transaction_amount`

```sql
SELECT customer_id, COUNT(*) AS high_value_txns
FROM transactions
WHERE transaction_amount > 5000
GROUP BY customer_id
HAVING COUNT(*) > 1;
```

**Explanation:**
- Filters high-value transactions (> $5000).
- Groups them by customer.
- Returns customers who've done this **more than once**.

---

### 13. Explain the difference between DELETE and TRUNCATE.

| Feature | DELETE | TRUNCATE |
|---------|--------|----------|
| Removes rows | Yes (can use WHERE condition) | Yes (removes all rows) |
| WHERE supported? | Yes | No |
| Logging | Logs each deleted row (slower) | Minimal logging (faster) |
| Rollback | Can be rolled back (if within transaction) | Can be rolled back (in some RDBMS) |
| Identity reset | Retains identity | Resets identity (in most DBs) |
| Use case | Partial deletion or audit trail needed | Full data wipe without audit needed |

---

### 14. How do you optimize SQL queries for better performance?

Here are **key SQL optimization techniques**:

**1. Use SELECT only required columns**
```sql
-- Bad
SELECT * FROM orders;
-- Good
SELECT order_id, customer_id FROM orders;
```

**2. Create proper indexes**
- Index frequently used columns in JOIN, WHERE, ORDER BY.

**3. Avoid functions on indexed columns**
```sql
-- Slower (cannot use index)
WHERE YEAR(order_date) = 2024
-- Better
WHERE order_date BETWEEN '2024-01-01' AND '2024-12-31'
```

**4. Use EXISTS instead of IN (for subqueries)**
```sql
-- Prefer EXISTS (better for large datasets)
SELECT name FROM customers c
WHERE EXISTS (
    SELECT 1 FROM orders o WHERE o.customer_id = c.customer_id
);
```

**5. Avoid unnecessary joins or nested subqueries**

**6. Use appropriate data types and avoid implicit conversions**

**7. Analyze execution plans (EXPLAIN or EXPLAIN ANALYZE)**

---

### 15. Write a query to find customers who haven't made any purchase in the last 6 months.

Assume:
- `customers(customer_id, name)`
- `transactions(customer_id, transaction_date)`

```sql
SELECT c.customer_id, c.name
FROM customers c
LEFT JOIN transactions t
    ON c.customer_id = t.customer_id
    AND t.transaction_date >= CURRENT_DATE - INTERVAL '6 months'
WHERE t.customer_id IS NULL;
```

**Explanation:**
- `LEFT JOIN` includes all customers.
- `WHERE t.customer_id IS NULL` ensures the customer had **no purchase in the last 6 months**.

---

### 16. How do you handle NULL values in SQL? Provide examples.

**NULL represents missing or unknown data.**

**1. Using IS NULL / IS NOT NULL:**
```sql
SELECT * FROM employees WHERE manager_id IS NULL;
```

**2. Replace NULL using COALESCE() or IFNULL() (MySQL):**
```sql
SELECT name, COALESCE(phone_number, 'Not Provided') AS contact
FROM customers;
```

**3. Handling NULLs in aggregation (e.g., AVG, SUM):**
- These functions **ignore NULLs by default**.
```sql
SELECT AVG(salary) FROM employees;
```

**4. Conditional checks:**
```sql
SELECT name,
    CASE
        WHEN salary IS NULL THEN 'Unknown'
        ELSE 'Known'
    END AS salary_status
FROM employees;
```

---

### 17. Write a query to transpose rows into columns.

Assume a table `sales` with: `region`, `month`, `sales_amount`

**Using CASE WHEN:**
```sql
SELECT region,
    SUM(CASE WHEN month = 'Jan' THEN sales_amount ELSE 0 END) AS Jan,
    SUM(CASE WHEN month = 'Feb' THEN sales_amount ELSE 0 END) AS Feb,
    SUM(CASE WHEN month = 'Mar' THEN sales_amount ELSE 0 END) AS Mar
FROM sales
GROUP BY region;
```

**Using PIVOT (SQL Server or Oracle syntax):**
```sql
SELECT region, [Jan], [Feb], [Mar]
FROM (
    SELECT region, month, sales_amount
    FROM sales
) AS src
PIVOT (
    SUM(sales_amount)
    FOR month IN ([Jan], [Feb], [Mar])
) AS p;
```

---

### 18. Explain indexing and how it improves query performance.

**What is an index?**
An **index** is a data structure that improves the speed of data retrieval operations on a database table at the cost of additional space and write-time performance.

**How indexing helps:**

| Feature | With Index | Without Index |
|---------|-----------|---------------|
| Search performance | Fast (uses binary/tree search) | Slow (scans every row — full scan) |
| Used in | WHERE, JOIN, ORDER BY, GROUP BY | Inefficient for large datasets |
| Types | B-tree (default), Bitmap, Hash, etc. | - |

**Example:**
```sql
-- Creating index
CREATE INDEX idx_customer_id ON transactions(customer_id);
```
- This helps queries like:
```sql
SELECT * FROM transactions WHERE customer_id = 101;
```

**Important notes:**
- Too many indexes can slow down INSERT/UPDATE.
- Avoid indexing columns with **low cardinality** (e.g., gender).
- Use **composite indexes** when querying multiple columns together.

---

### 19. Write a query to fetch the maximum transaction amount per customer.

Assume `transactions` table has:
- `customer_id` — ID of the customer
- `transaction_id` — Unique transaction ID
- `amount` — Transaction amount

```sql
SELECT customer_id, MAX(amount) AS max_transaction
FROM transactions
GROUP BY customer_id;
```

**Explanation:**
- `GROUP BY` groups all transactions by customer.
- `MAX(amount)` returns the highest transaction for each group (customer).

---

### 20. What is a self-join, and how is it used?

**Definition:**
A **self-join** is a regular join where a table is joined with itself. It is useful when rows in a table are related to other rows in the same table.

**Example Use Case – Employees and Managers:**

Assume:

| emp_id | name  | manager_id |
|--------|-------|------------|
| 1      | Alice | NULL       |
| 2      | Bob   |            |
| 3      | Carol | 1          |
| 4      | David | 2          |

Here, `manager_id` refers to `emp_id` of another employee.

**Query: Get employee names along with their manager names**
```sql
SELECT e.name AS employee_name, m.name AS manager_name
FROM employees e
LEFT JOIN employees m
    ON e.manager_id = m.emp_id;
```

**Explanation:**
- `e` is an alias for employees (as employee).
- `m` is another alias for the same table (as manager).
- The join links an employee to their manager using `manager_id = emp_id`.

---

## Data Analysis / Scenario-Based Questions

*(Additional scenario-based questions section — to be continued)*
