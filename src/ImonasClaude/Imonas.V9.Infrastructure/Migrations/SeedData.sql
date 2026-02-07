-- Imonas V9 Seed Data
-- Sample data for testing and initial setup

-- Insert sample users
INSERT INTO users (id, username, email, is_active, two_factor_enabled, created_at, created_by) VALUES
(uuid_generate_v4(), 'admin', 'admin@imonas.com', true, false, CURRENT_TIMESTAMP, 'System'),
(uuid_generate_v4(), 'analyst', 'analyst@imonas.com', true, false, CURRENT_TIMESTAMP, 'System'),
(uuid_generate_v4(), 'viewer', 'viewer@imonas.com', true, false, CURRENT_TIMESTAMP, 'System');

-- Insert sample user roles
INSERT INTO user_roles (id, user_id, role_name, created_at, created_by)
SELECT uuid_generate_v4(), u.id, 'Admin', CURRENT_TIMESTAMP, 'System'
FROM users u WHERE u.username = 'admin';

INSERT INTO user_roles (id, user_id, role_name, created_at, created_by)
SELECT uuid_generate_v4(), u.id, 'Analyst', CURRENT_TIMESTAMP, 'System'
FROM users u WHERE u.username = 'analyst';

INSERT INTO user_roles (id, user_id, role_name, created_at, created_by)
SELECT uuid_generate_v4(), u.id, 'Viewer', CURRENT_TIMESTAMP, 'System'
FROM users u WHERE u.username = 'viewer';

-- Insert sample PSP profiles
INSERT INTO psp_profiles (id, psp_name, psp_code, file_format, time_zone, supported_currencies, is_active, created_at, created_by) VALUES
(uuid_generate_v4(), 'Stripe', 'STRIPE', 'CSV', 'UTC', 'USD,EUR,GBP', true, CURRENT_TIMESTAMP, 'System'),
(uuid_generate_v4(), 'PayPal', 'PAYPAL', 'CSV', 'UTC', 'USD,EUR,GBP', true, CURRENT_TIMESTAMP, 'System'),
(uuid_generate_v4(), 'Adyen', 'ADYEN', 'JSON', 'UTC', 'USD,EUR,GBP,AUD', true, CURRENT_TIMESTAMP, 'System'),
(uuid_generate_v4(), 'Worldpay', 'WORLDPAY', 'CSV', 'Europe/London', 'USD,EUR,GBP', true, CURRENT_TIMESTAMP, 'System');

-- Insert sample status mappings
INSERT INTO status_mappings (id, psp_profile_id, psp_status, canonical_status, description, version, is_active, created_at, created_by)
SELECT uuid_generate_v4(), p.id, 'succeeded', 'Completed', 'Payment succeeded', '1.0', true, CURRENT_TIMESTAMP, 'System'
FROM psp_profiles p WHERE p.psp_code = 'STRIPE';

INSERT INTO status_mappings (id, psp_profile_id, psp_status, canonical_status, description, version, is_active, created_at, created_by)
SELECT uuid_generate_v4(), p.id, 'failed', 'Failed', 'Payment failed', '1.0', true, CURRENT_TIMESTAMP, 'System'
FROM psp_profiles p WHERE p.psp_code = 'STRIPE';

INSERT INTO status_mappings (id, psp_profile_id, psp_status, canonical_status, description, version, is_active, created_at, created_by)
SELECT uuid_generate_v4(), p.id, 'pending', 'Pending', 'Payment pending', '1.0', true, CURRENT_TIMESTAMP, 'System'
FROM psp_profiles p WHERE p.psp_code = 'STRIPE';

-- Insert sample matching rules
INSERT INTO matching_rules (id, rule_name, description, rule_type, rule_definition, priority, is_active, stop_at_first_match, version, effective_from, created_at, created_by) VALUES
(uuid_generate_v4(), 'Exact Transaction ID Match', 'Match on exact transaction ID', 1, '{"field": "TransactionId", "operator": "equals"}', 1, true, true, '1.0.0', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'System'),
(uuid_generate_v4(), 'Composite Key Match', 'Match on Amount + Currency + AuthCode', 2, '{"fields": ["Amount", "Currency", "AuthCode"], "operator": "all_equal"}', 2, true, true, '1.0.0', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'System'),
(uuid_generate_v4(), 'Amount Tolerance Match', 'Match with 0.01 amount tolerance', 3, '{"field": "Amount", "tolerance": 0.01, "time_window_days": 1}', 3, true, false, '1.0.0', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'System');

-- Insert sample fee contract
INSERT INTO fee_contracts (id, psp_profile_id, contract_name, payment_method, brand, currency, fee_structure, fixed_fee, percentage_fee, minimum_fee, maximum_fee, effective_from, version, status, created_at, created_by)
SELECT
    uuid_generate_v4(),
    p.id,
    'Stripe Standard Fees',
    'card',
    'Visa',
    'USD',
    'hybrid',
    0.30,
    0.029,
    0.30,
    NULL,
    CURRENT_TIMESTAMP,
    '1.0',
    'Active',
    CURRENT_TIMESTAMP,
    'System'
FROM psp_profiles p WHERE p.psp_code = 'STRIPE';

-- Insert sample reconciliation run
INSERT INTO reconciliation_runs (id, run_name, status, started_at, completed_at, rule_pack_version, total_records, matched_records, unmatched_records, partial_match_records, match_percentage, is_replayable, created_at, created_by) VALUES
(uuid_generate_v4(), 'Sample Run - 2026-01-12', 3, CURRENT_TIMESTAMP - INTERVAL '1 hour', CURRENT_TIMESTAMP - INTERVAL '30 minutes', '1.0.0', 1000, 950, 40, 10, 95.00, true, CURRENT_TIMESTAMP, 'System');

-- Insert sample cases
INSERT INTO cases (id, case_number, title, description, status, severity, variance_type, assigned_to, created_at, created_by) VALUES
(uuid_generate_v4(), 'CASE-2026-0001', 'Amount Variance - Transaction ABC123', 'Transaction amount differs by $0.50 between PSP and bank', 1, 2, 1, 'analyst', CURRENT_TIMESTAMP, 'System'),
(uuid_generate_v4(), 'CASE-2026-0002', 'Missing Settlement - PayPal', 'Expected settlement not received from PayPal', 1, 3, 2, 'analyst', CURRENT_TIMESTAMP, 'System'),
(uuid_generate_v4(), 'CASE-2026-0003', 'Currency Mismatch - EUR vs USD', 'Transaction currency mismatch between systems', 2, 2, 3, 'analyst', CURRENT_TIMESTAMP, 'System');

-- Add comments to one of the cases
INSERT INTO case_comments (id, case_id, comment, commented_by, created_at, created_by)
SELECT uuid_generate_v4(), c.id, 'Investigating with PSP support team', 'analyst', CURRENT_TIMESTAMP, 'analyst'
FROM cases c WHERE c.case_number = 'CASE-2026-0001';

-- Insert sample audit log entries
INSERT INTO audit_logs (id, action, entity_type, performed_by, created_at, created_by) VALUES
(uuid_generate_v4(), 'Login', 'User', 'admin', CURRENT_TIMESTAMP, 'System'),
(uuid_generate_v4(), 'ViewDashboard', 'Dashboard', 'admin', CURRENT_TIMESTAMP, 'System'),
(uuid_generate_v4(), 'CreateCase', 'Case', 'analyst', CURRENT_TIMESTAMP, 'System');

COMMIT;
