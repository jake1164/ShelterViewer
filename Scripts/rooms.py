import json
import os
import glob

# Load rooms.json
with open('ShelterViewer.Shared/Data/rooms.json') as f:
    rooms = json.load(f)

# Extract room types from rooms.json
room_types = {room['type'] for room in rooms['rooms']}

# Load this room with found rooms.
matched_rooms = set() # In both rooms.json and vaults
unmatched_rooms = set() # rooms not found in vaults
unmatched_vaults = set() # vaults not found In rooms.json


# vaults are found in ~/Downloads/vaults, iterate through all vaults *.json
vaults_path = os.path.expanduser('~/Downloads/vaults/*.json')
for vault_file in glob.glob(vaults_path):
    print('Processing', vault_file)
    with open(vault_file) as f:
        vault_data = json.load(f)

    # Extract acceptedRoom values from each vault file
    for vault_room in vault_data['vault']['rooms']:
        if vault_room['type'] in room_types:
            matched_rooms.add(vault_room['type'])# matching rooms
        else: 
            unmatched_vaults.add(vault_room['type'])


# Find unmatched room types
unmatched_rooms = room_types - matched_rooms
print('Matched rooms {}, Unmatched {}'.format(len(matched_rooms), len(unmatched_rooms)))
print("\nUnmatched vault types:", sorted(unmatched_vaults))
#print("\nMatched Rooms:", sorted(matched_rooms))
print('\nunused rooms', sorted(unmatched_rooms))