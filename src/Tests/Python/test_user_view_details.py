#!/usr/bin/env python
# -*- coding: utf-8 -*-

import unittest
from pprint import pprint
from third_party.ddt.ddt import ddt, data, file_data, unpack

from models.api_facade import ApiFacade
from models.response_validator import ResponseValidator

TEST_USER = {
			"displayName": "John Doe", 
			"email": "johndoe@test.com", 
			"password": "12345678",
			"level": "Investor",
			"accounts": [
					{
					"name": "Default Account", 
					"balance": 1000000
					}
			],
			"watchlists": [
					{
					"name": "Default Watchlist"
					}
			]
		}

TOKEN = None

class UserViewDetailsTestCase(unittest.TestCase):
	@classmethod
	def setUpClass(cls):
		global TOKEN
		cls.api = ApiFacade()
		response_code = cls.api.register_user(TEST_USER["displayName"], TEST_USER["email"], TEST_USER["password"])
		response_code, TOKEN = cls.api.authenticate_user(TEST_USER["email"], TEST_USER["password"])

	@classmethod
	def tearDownClass(cls):
		global TOKEN
		cls.api.delete_user(TOKEN)

	def setUp(self):
		pass

	def tearDown(self):
		pass

	def test_viewDetails_success(self):
		"""An authenticated user can view their user account details"""
		expected_response_code = 200
		input_data = ("John Doe", "johndoe@test.com", "12345678")
		account_model = {
			"id": {"key_only": True, "is_collection": False},
			"name": {"key_only": False, "is_collection": False, "value": "Default Account"},
			"balance": {"key_only": False, "is_collection": False, "value": 1000000}
		}

		watchlist_model = {
			"id": {"key_only": True, "is_collection": False},
			"name": {"key_only": False, "is_collection": False, "value": "Default Watchlist"}
		}

		model = {
				"id": {"key_only": True, "is_collection": False},
				"email": {"key_only": False, "is_collection": False, "value": TEST_USER["email"]},
				"displayName": {"key_only": False, "is_collection": False, "value": TEST_USER["displayName"]},
				"level": {"key_only": False, "is_collection": False, "value": TEST_USER["level"]},
				"accounts": {"key_only": False, "is_collection": True, "model": account_model},
				"watchlists": {"key_only": False, "is_collection": True, "model": watchlist_model}
		}
		api = ApiFacade()
		response = api.view_details(TOKEN)
		validator = ResponseValidator(response, expected_response_code, model)
		correct_status, status = validator.response_code_success()
		correct_body = validator.response_body_success()

		self.assertEqual(correct_status, True, msg = "On data [{0}][{1}][{2}] - {3}".format(*input_data, validator.get_errors()))
		self.assertEqual(correct_body, True, msg = "On data [{0}][{1}][{2}] - {3}".format(*input_data, validator.get_errors()))

	def test_deletion_userIsNotAuthenticated(self):
		"""An unauthenticated user cannot delete their user account"""
		expected_response_code = 401
		input_data = ("John Doe", "johndoe@test.com", "12345678")
		model = None
		api = ApiFacade()
		response = api.view_details(token = None)
		validator = ResponseValidator(response, expected_response_code, model)
		correct_status, status = validator.response_code_success()
		correct_body = validator.response_body_success()

		self.assertEqual(correct_status, True, msg = "On data [{0}][{1}][{2}] - {3}".format(*input_data, validator.get_errors()))
		self.assertEqual(correct_body, True, msg = "On data [{0}][{1}][{2}] - {3}".format(*input_data, validator.get_errors()))

if __name__ == "__main__":
	unittest.main()