import json
import sys
import random
import os

FULL_CARDS_PATH = '../front/cards-full.json'
CARDS_PATH = '../front/cards.json'

data = json.loads(open(FULL_CARDS_PATH, 'r', encoding='utf-8').read())
index = {}
for card in data:
    index[card['name']] = card
txt_file = sys.argv[1]
lines = open(txt_file, 'r', encoding='utf-8').read().split('\n')
result = []
for cname in lines:
    if cname not in index:
        print('CARD ' + cname + ' NOT ADDED!')
        continue
    result += [index[cname]]
open(CARDS_PATH, 'w').write(json.dumps(result, indent=4))