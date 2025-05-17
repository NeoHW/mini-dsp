import json
import random

def generate_user(user_id):
    gender = random.choice(["Male", "Female"])
    age_band = random.choice(["AgeBand25To35", "AgeBand35To50"])
    location = random.choice(["LivesInMetroArea", "LivesInRegionalArea"])
    
    targeting_data = [gender, age_band, location]
    
    if random.random() < 0.5:
        targeting_data.append("HighIncomeCategory")
    
    return {
        "UserId": f"user-{user_id:03}",
        "TargetingData": targeting_data
    }

def generate_users(n):
    return [generate_user(i + 1) for i in range(n)]

def save_to_file(users, filename):
    with open(filename, "w") as f:
        json.dump(users, f, indent=2)

if __name__ == "__main__":
    num_users = 30
    users = generate_users(num_users)
    save_to_file(users, "users.json")
    print(f"users.json file generated with {num_users} users.")
