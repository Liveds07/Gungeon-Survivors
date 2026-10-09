"""Read the companion's installed assets into a private, disposable runtime cache.
Never include the cache in a package. Requires UnityPy in a local Python runtime.
"""
import argparse
import hashlib
import json
from pathlib import Path
import UnityPy


def read_content(companion, sheets, output):
    root = Path(companion).resolve()
    data_root = root / 'VampireSurvivors_Data'
    resource = data_root / 'resources.assets'
    if not (root / 'VampireSurvivors.exe').is_file() or not resource.is_file():
        raise ValueError('A complete installed Vampire Survivors copy is required')
    out = Path(output).resolve()
    project = Path(__file__).resolve().parents[1]
    if out == project or project in out.parents or out == root or root in out.parents:
        raise ValueError('Runtime cache must be outside the source project and game installation')
    config = json.loads(Path(sheets).read_text(encoding='utf-8-sig'))
    env = UnityPy.load(str(resource))
    tables = {}
    sprites = {}
    for obj in env.objects:
        if obj.type.name == 'TextAsset':
            item = obj.read()
            if item.m_Name in ('CHARACTER_DATA', 'WEAPON_DATA'):
                tables[item.m_Name] = json.loads(item.m_Script)
        elif obj.type.name == 'Sprite':
            item = obj.read()
            sprites.setdefault(item.m_Name, item)
    # The installed Imelda sheet is addressable, rather than embedded in resources.
    bundles = data_root / 'StreamingAssets' / 'aa' / 'StandaloneWindows64'
    for survivor in config['sheets']['survivors']['rows']:
        char_key = survivor['data_selector'].rsplit('/', 1)[-1]
        character = tables['CHARACTER_DATA'][char_key][0]
        sprite_name = character['spriteName'].removesuffix('.png')
        bundle = bundles / ('vs_local_assets_' + character['textureName'] + '.bundle')
        if not bundle.is_file():
            raise ValueError(f'Missing installed sprite container: {bundle.name}')
        e = UnityPy.load(str(bundle))
        for obj in e.objects:
            if obj.type.name == 'Sprite':
                item = obj.read()
                sprites.setdefault(item.m_Name, item)
    out.mkdir(parents=True, exist_ok=True)
    result = {'survivors': [], 'weapons': [], 'accessories': [], 'challenges': config['sheets']['challenges']['rows'],
              'rewards': config['sheets']['rewards']['rows'], 'settings': config['sheets']['slice']['rows'][0],
              'ui': config['sheets']['ui']['rows'][0], 'source': str(root), 'sourceHash': hashlib.sha256(resource.read_bytes()).hexdigest()}
    for row in config['sheets']['survivors']['rows']:
        key = row['data_selector'].rsplit('/', 1)[-1]
        levels = tables['CHARACTER_DATA'][key]
        base = levels[0]
        sprite_name = base['spriteName'].removesuffix('.png')
        sprites[sprite_name].image.save(out / (row['id'] + '.png'))
        prefix, number = sprite_name.rsplit('_', 1)
        frame_ids = []
        for index in range(1, int(base['walkingFrames']) + 1):
            name = prefix + '_' + str(index).zfill(len(number))
            frame_id = row['id'] + '_walk_' + str(index)
            sprites[name].image.save(out / (frame_id + '.png'))
            frame_ids.append(frame_id)
        result['survivors'].append({'id': row['id'], 'name': base['charName'], 'weaponId': row['weapon_id'],
                                   'power': base['power'], 'growth': base['growth'], 'walkFrames': frame_ids,
                                   'levels': [{'level': r.get('level', 0), 'power': r.get('power', 0),
                                               'growth': r.get('growth', 0)} for r in levels[1:]]})
    for row in config['sheets']['weapons']['rows']:
        key = row['data_selector'].rsplit('/', 1)[-1]
        levels = tables['WEAPON_DATA'][key]
        evo = tables['WEAPON_DATA'][row['evolution_selector'].rsplit('/', 1)[-1]][0]
        for target, base in ((row['id'], levels[0]), (row['id'] + '_evolved', evo)):
            sprite_name = base['frameName'].removesuffix('.png')
            sprites[sprite_name].image.save(out / (target + '.png'))
        effect_names = row['effect_selector'].rsplit('/', 1)[-1].split(';')
        effect_ids = ['whip_effect'] if row['id'] == 'whip' else ['wand_effect_1', 'wand_effect_2']
        if len(effect_names) != len(effect_ids):
            raise ValueError('Unexpected attack frame count in sheet')
        for effect_id, name in zip(effect_ids, effect_names):
            sprites[name].image.save(out / (effect_id + '.png'))
        # Data stays in the cache; the source contains selectors and conversion logic only.
        result['weapons'].append({'id': row['id'], 'name': levels[0]['name'], 'evolutionName': evo['name'],
                                  'effectFrames': effect_ids, 'requiredPassive': evo['requires'][0], 'repeatInterval': levels[0]['repeatInterval'] / 1000,
                                  'levels': [{k: r.get(k, 0) for k in ('power', 'interval', 'amount', 'area', 'penetrating', 'speed')}
                                             for r in levels],
                                  'evolution': {k: evo.get(k, 0) for k in ('power', 'interval', 'amount', 'area', 'penetrating', 'speed', 'critChance', 'critMul')}})
    for row in config['sheets']['accessories']['rows']:
        key = row['data_selector'].rsplit('/', 1)[-1]
        levels = tables['WEAPON_DATA'][key]
        if not levels[0].get('isPowerUp'):
            raise ValueError('Accessory selector is not an installed passive')
        sprites[levels[0]['frameName'].removesuffix('.png')].image.save(out / (row['id'] + '.png'))
        result['accessories'].append({'id': row['id'], 'dataKey': key, 'name': levels[0]['name'],
                                     'description': levels[0]['description'],
                                     'levels': [{k: r.get(k, 0) for k in ('power', 'area', 'amount', 'growth', 'cooldown', 'maxHp')} for r in levels]})
    required = {w['requiredPassive'] for w in result['weapons']}
    available = {a['dataKey'] for a in result['accessories']}
    if not required.issubset(available):
        raise ValueError('Missing sheet-defined evolution accessory')
    sprites[result['ui']['sprite_selector'].rsplit('/', 1)[-1]].image.save(out / 'ui_frame.png')
    temp = out / 'content.json.tmp'
    temp.write_text(json.dumps(result), encoding='utf-8')
    temp.replace(out / 'content.json')
    print('Read companion data, walking frames, attack sprites and UI frame into private cache; source SHA256 ' + result['sourceHash'])
    return result


if __name__ == '__main__':
    p = argparse.ArgumentParser()
    p.add_argument('--companion', required=True)
    p.add_argument('--sheets', required=True)
    p.add_argument('--output', required=True)
    a = p.parse_args()
    read_content(a.companion, a.sheets, a.output)
