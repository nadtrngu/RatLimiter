                         +----------------+
                         |     Client     |
                         +--------+-------+
                                  |
                                  v
                        +---------+----------+
                        |     API Gateway    |
                        +---------+----------+
                                  |
                                  v
                     +------------+-------------+
                     |   Lambda (RateLimiter)   |
                     |   - Token Bucket         |
                     |   - Sliding Window       |
                     |   - API Key Logic        |
                     +------------+-------------+
                                  |
                                  v
                           +------+--------+
                           |    Redis      |
                           | (State Store) |
                           +---------------+
