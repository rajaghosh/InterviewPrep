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

### S1. Order volumes dropped 30% in Bangalore last week. How do you investigate?

**Approach:**

1. **Isolate the dimension** — Is it all of Bangalore or specific areas/cuisines?
```sql
SELECT city, zone, cuisine_type, COUNT(*) AS orders,
       LAG(COUNT(*)) OVER (PARTITION BY city, zone ORDER BY week) AS prev_week
FROM orders
WHERE city = 'Bangalore'
GROUP BY city, zone, cuisine_type, week
ORDER BY week DESC;
```

2. **Check supply side** — Did active restaurant count or delivery partner availability drop?
```sql
SELECT date, COUNT(DISTINCT restaurant_id) AS active_restaurants,
       COUNT(DISTINCT delivery_partner_id) AS active_partners
FROM activity_log
WHERE city = 'Bangalore'
GROUP BY date ORDER BY date DESC;
```

3. **Check demand side** — Did app sessions, search queries, or cart additions drop?
```sql
SELECT date, event_type, COUNT(*) AS events
FROM app_events
WHERE city = 'Bangalore' AND event_type IN ('session_start', 'search', 'add_to_cart', 'order_placed')
GROUP BY date, event_type ORDER BY date DESC;
```

4. **Funnel analysis** — Where in the funnel are users dropping off?
```
Sessions → Search → Restaurant View → Add to Cart → Checkout → Order Placed
```

5. **Hypothesis checklist:**
   - Competitor promotion in Bangalore (check social/marketing data)
   - Payment gateway failure (check payment success rate)
   - Weather event (high cancellations, low partner availability)
   - App update with bug (check crash logs, filter by app version)
   - Pricing change (check average order value trend)

**Conclusion format:** "The drop is localized to [zone X], driven by [supply/demand/funnel issue], starting on [date], likely caused by [hypothesis with supporting data]."

---

### S2. Zomato wants to reduce late deliveries. How do you measure and improve on-time delivery rate?

**Define the metric first:**
```
On-Time Delivery Rate = (Orders delivered within promised ETA) / (Total delivered orders) × 100
```

**SQL to compute it:**
```sql
SELECT
  DATE(order_placed_at) AS date,
  city,
  ROUND(100.0 * SUM(CASE WHEN actual_delivery_time <= promised_delivery_time THEN 1 ELSE 0 END)
        / COUNT(*), 2) AS on_time_rate_pct,
  AVG(actual_delivery_time - promised_delivery_time) AS avg_delay_minutes
FROM deliveries
WHERE status = 'delivered'
GROUP BY DATE(order_placed_at), city
ORDER BY date DESC;
```

**Root cause segmentation:**
```sql
SELECT
  CASE
    WHEN prep_time > estimated_prep_time + 5 THEN 'Restaurant delay'
    WHEN pickup_to_delivery_time > estimated_transit_time + 5 THEN 'Transit delay'
    WHEN assignment_time > 3 THEN 'Partner assignment delay'
    ELSE 'On time'
  END AS delay_cause,
  COUNT(*) AS order_count,
  ROUND(100.0 * COUNT(*) / SUM(COUNT(*)) OVER (), 1) AS pct_of_total
FROM deliveries
WHERE status = 'delivered'
GROUP BY delay_cause ORDER BY order_count DESC;
```

**Improvement levers:**
- Smarter ETA prediction (ML model using historical prep times + real-time traffic)
- Partner pre-positioning based on order demand heatmaps
- Restaurant performance SLA alerts and scoring
- Dynamic ETA re-estimation with in-app updates for customers

---

### S3. How would you design an A/B test for a new Zomato feature: "Dish-level reviews"?

**Objective:** Measure whether dish-level reviews increase order conversion rate.

**Experiment design:**

| Element | Decision |
|---------|----------|
| **Unit of randomization** | User ID (not session — dish reviews affect repeat users most) |
| **Assignment** | 50/50 random split, stratified by city tier and order frequency |
| **Control** | Existing restaurant-level reviews |
| **Treatment** | Dish-level photo + rating alongside each menu item |
| **Primary metric** | Order conversion rate (sessions → order placed) |
| **Secondary metrics** | Average order value, repeat order rate, time-in-app |
| **Guardrail metrics** | App crash rate, page load time (must not degrade) |
| **Min detectable effect** | 2% relative uplift in conversion (based on historical variance) |
| **Sample size** | Calculate via power analysis: ~500K users per group for 80% power |
| **Duration** | 2 weeks minimum (captures weekday + weekend behavior) |

**SQL to monitor during the test:**
```sql
SELECT
  experiment_group,
  COUNT(DISTINCT user_id) AS users,
  COUNT(DISTINCT CASE WHEN event_type = 'order_placed' THEN session_id END) AS converting_sessions,
  COUNT(DISTINCT session_id) AS total_sessions,
  ROUND(100.0 * COUNT(DISTINCT CASE WHEN event_type = 'order_placed' THEN session_id END)
        / COUNT(DISTINCT session_id), 2) AS conversion_rate_pct
FROM ab_test_events
WHERE experiment_name = 'dish_level_reviews'
GROUP BY experiment_group;
```

**Decision framework:**
- Statistical significance at p < 0.05 before calling a winner
- Run for full 2 weeks — no peeking/stopping early (avoids p-hacking)
- If guardrail metrics degrade → stop immediately regardless of primary metric

---

### S4. Customer complaints about incorrect charges have tripled. How do you investigate?

**Step 1 — Quantify and segment:**
```sql
SELECT
  complaint_type,
  payment_method,
  DATE(complaint_raised_at) AS date,
  COUNT(*) AS complaints,
  AVG(disputed_amount) AS avg_amount
FROM customer_complaints
WHERE complaint_type = 'incorrect_charge'
  AND complaint_raised_at >= CURRENT_DATE - INTERVAL '30 days'
GROUP BY complaint_type, payment_method, DATE(complaint_raised_at)
ORDER BY date DESC;
```

**Step 2 — Find the spike date:**
The tripling is a step change — look for the exact date it started. Cross-reference with:
- Deployment logs (was there a billing code change?)
- Payment gateway changelog
- Promo/discount system changes

**Step 3 — Check double-charge pattern:**
```sql
-- Find users charged more than once for same order
SELECT user_id, order_id, COUNT(*) AS charge_count, SUM(amount) AS total_charged
FROM payment_transactions
WHERE status = 'success'
GROUP BY user_id, order_id
HAVING COUNT(*) > 1;
```

**Step 4 — Check refund gap:**
```sql
SELECT
  DATE(order_placed_at) AS date,
  COUNT(*) AS total_orders,
  SUM(CASE WHEN refund_issued = TRUE THEN 1 ELSE 0 END) AS refunds,
  ROUND(100.0 * SUM(CASE WHEN refund_issued = TRUE THEN 1 ELSE 0 END) / COUNT(*), 2) AS refund_rate
FROM orders
GROUP BY date ORDER BY date DESC;
```

**Resolution:** Match the tripling date to a specific system change, quantify affected users, compute total exposure, and provide a targeted refund run.

---

### S5. Zomato's Gold membership retention is 60% at 3 months. How do you improve it?

**Define the cohort:**
```sql
-- 3-month retention: members who subscribed in month M still active in month M+3
SELECT
  DATE_TRUNC('month', subscription_start) AS cohort_month,
  COUNT(DISTINCT user_id) AS cohort_size,
  COUNT(DISTINCT CASE WHEN subscription_end > cohort_start + INTERVAL '3 months'
                      OR subscription_end IS NULL THEN user_id END) AS retained_at_3m,
  ROUND(100.0 * COUNT(DISTINCT CASE WHEN subscription_end > cohort_start + INTERVAL '3 months'
                      OR subscription_end IS NULL THEN user_id END) / COUNT(DISTINCT user_id), 1) AS retention_pct
FROM gold_subscriptions
GROUP BY cohort_month ORDER BY cohort_month;
```

**Segment churned users:**
```sql
SELECT
  AVG(orders_during_subscription) AS avg_orders,
  AVG(discount_used_amount) AS avg_discount_used,
  AVG(days_to_first_post_signup_order) AS days_to_first_order,
  cancellation_reason
FROM gold_subscriptions
WHERE subscription_end BETWEEN subscription_start AND subscription_start + INTERVAL '3 months'
GROUP BY cancellation_reason ORDER BY COUNT(*) DESC;
```

**Key levers:**
- **Activation speed** — users who order within 3 days of signup retain significantly better; trigger onboarding nudge
- **Perceived value** — if discount_used_amount is low, users don't feel the benefit; surface Gold savings in app
- **Renewal reminder** — proactive reminder 7 days before renewal with savings summary
- **Win-back campaign** — for users who cancelled, offer 1-month discounted re-activation

**North Star improvement:** Move first-order-within-48-hours rate from X% → X+15%, measure 3-month retention delta in A/B test.
