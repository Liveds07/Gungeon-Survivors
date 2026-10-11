import copy
import json
import unittest
from tools.preflight import SHEETS, validate
from tools.routes import plan


class DesignTests(unittest.TestCase):
    def setUp(self):
        self.document = json.loads(SHEETS.read_text())

    def test_design_cells_and_references_are_complete(self):
        self.assertEqual([], validate(self.document, "design"))

    def test_release_cannot_pass_without_game_inspection(self):
        errors = validate(self.document)
        self.assertTrue(any("unfilled" in e for e in errors))
        self.assertTrue(any("not verified in game" in e for e in errors))

    def test_broken_reward_reference_is_rejected(self):
        self.document["sheets"]["challenges"]["rows"][0]["reward_id"] = "missing"
        self.assertTrue(any("unresolved reference" in e for e in validate(self.document, "design")))

    def test_every_route_rejoins_and_keeps_exploration(self):
        for seed in range(1000):
            route = plan(seed, self.document)
            self.assertEqual(len(route), 4)
            for stage in route:
                self.assertTrue(stage["normal_route"])
                self.assertEqual(stage["rejoin_stage"], stage["stage"] + 1)
                choices = stage["challenges"]
                self.assertEqual(len({c["id"] for c in choices}), 2)
                self.assertEqual(len({c["reward_id"] for c in choices}), 2)

    def test_seed_is_repeatable_and_routes_vary(self):
        before = copy.deepcopy(self.document)
        self.assertEqual(plan(123, self.document), plan(123, self.document))
        signatures = {json.dumps(plan(seed, self.document), sort_keys=True) for seed in range(1000)}
        self.assertGreater(len(signatures), 100)
        self.assertEqual(before, self.document)

    def test_impossible_choice_count_is_rejected(self):
        self.document["sheets"]["systems"]["rows"][0]["choices_per_stage"] = 4
        with self.assertRaises(ValueError):
            plan(0, self.document)

    def test_survival_cover_preserves_walkable_arena_and_entry(self):
        arena = self.document['sheets']['survival_rooms']['rows'][0]
        width, height = arena['arena_width'], arena['arena_height']
        walls = set()
        for block in arena['wall_rectangles'] + arena['column_rectangles']:
            for x in range(block['x'], block['x'] + block['width']):
                for y in range(block['y'], block['y'] + block['height']):
                    self.assertTrue(2 <= x < width - 2 and 6 <= y < height - 2)
                    walls.add((x, y))
        floor = {(x, y) for x in range(width) for y in range(height)} - walls
        reachable = {(width // 2, 0)}
        pending = list(reachable)
        while pending:
            x, y = pending.pop()
            for point in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
                if point in floor and point not in reachable:
                    reachable.add(point)
                    pending.append(point)
        self.assertEqual(floor, reachable, 'Cover must not isolate an enemy spawn or player route')
        self.assertIn((width // 2, height // 2), reachable, 'Reward position stays reachable')
        for table in arena['table_positions']:
            self.assertIn((table['x'], table['y']), reachable)
