import json
import itertools
import random
import uuid

TARGETING_DATA = [
    "Male",
    "Female",
    "HighIncomeCategory",
    "AgeBand25To35",
    "AgeBand35To50",
    "LivesInMetroArea",
    "LivesInRegionalArea"
]

NUM_CAMPAIGNS = 5
NUM_FILES = 3

def generate_all_combinations():
    combinations = []
    for r in range(1, len(TARGETING_DATA) + 1):
        combinations.extend(itertools.combinations(TARGETING_DATA, r))
    return combinations

def generate_campaign():
    bid_lines = []
    for combi in generate_all_combinations():
        bid_line = {
            "TargetingData": list(combi),
            "BidFactor": round(random.uniform(1.1, 4.0), 2)
        }
        bid_lines.append(bid_line)
    return {
        "CampaignId": str(uuid.uuid4()),
        "Budget": 100000,
        "RemainingBudget": 100000,
        "BaseBid": 100,
        "BidLines": bid_lines
    }

def generate_campaign_set():
    return [generate_campaign() for _ in range(NUM_CAMPAIGNS)]
    
def save_to_file(data, filename):
   with open(filename, "w") as f:
       json.dump(data, f, indent=2)

def main():
   for i in range(1, NUM_FILES + 1):
       campaigns = generate_campaign_set()
       save_to_file(campaigns, f"campaign_{i}.json")
       print(f"Saved campaign_{i}.json")

if __name__ == "__main__":
   main()