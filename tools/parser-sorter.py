import json
import sys
import random

PATH = sys.argv[1]

data = json.loads(open(PATH, 'r', encoding='utf-8').read())
other = []
effects = []
in_effects = []
costs = []
in_costs = []
events = []
in_events = []
for item in data['templates']:
    if item.endswith('in-effect.xml'):
        in_effects += [item]
        continue
    if item.endswith('in-cost.xml'):
        in_costs += [item]
        continue
    if item.endswith('in-event.xml'):
        in_events += [item]
        continue
    if item.endswith('-effect.xml'):
        effects += [item]
        continue
    if item.endswith('-cost.xml'):
        costs += [item]
        continue
    if item.endswith('-event.xml'):
        events += [item]
        continue
    other += [item]
data['templates'] = other + effects + in_effects + costs + in_costs + events + in_events
open(PATH, 'w').write(json.dumps(data, indent=4))