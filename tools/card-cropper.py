import json
import sys
import random

FULL_CARDS_PATH = '../front/cards-full.json'
CARDS_PATH = '../front/cards.json'

data = json.loads(open(FULL_CARDS_PATH, 'r', encoding='utf-8').read())
amount = int(sys.argv[1])
data = random.sample(data, amount)
open(CARDS_PATH, 'w').write(json.dumps(data, indent=4))