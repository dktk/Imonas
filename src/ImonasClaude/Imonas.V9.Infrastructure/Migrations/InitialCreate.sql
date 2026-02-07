-- Imonas V9 Initial Database Schema for PostgreSQL
-- Bronze/Silver/Gold Layered Architecture

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Bronze Layer Tables
CREATE TABLE bronze_files (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    file_name VARCHAR(500) NOT NULL,
    file_path VARCHAR(1000) NOT NULL,
    file_size_bytes BIGINT NOT NULL,
    file_hash VARCHAR(128) NOT NULL,
    status INT NOT NULL,
    uploaded_at TIMESTAMP NOT NULL,
    uploaded_by VARCHAR(255) NOT NULL,
    error_message TEXT,
    rejection_reason TEXT,
    psp_profile_id UUID,
    source_system VARCHAR(255) NOT NULL,
    source_id VARCHAR(255) NOT NULL,
    run_id UUID NOT NULL,
    rule_pack_version VARCHAR(50) NOT NULL,
    content_hash VARCHAR(128) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

CREATE INDEX idx_bronze_files_hash ON bronze_files(file_hash);
CREATE INDEX idx_bronze_files_run_id ON bronze_files(run_id);

CREATE TABLE bronze_records (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    bronze_file_id UUID NOT NULL REFERENCES bronze_files(id),
    row_number INT NOT NULL,
    raw_data TEXT NOT NULL,
    parsed_data TEXT,
    is_valid BOOLEAN NOT NULL DEFAULT false,
    validation_errors TEXT,
    source_id VARCHAR(255) NOT NULL,
    run_id UUID NOT NULL,
    rule_pack_version VARCHAR(50) NOT NULL,
    content_hash VARCHAR(128) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

CREATE INDEX idx_bronze_records_file ON bronze_records(bronze_file_id);

-- Silver Layer Tables
CREATE TABLE silver_transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    transaction_id VARCHAR(255) NOT NULL,
    external_transaction_id VARCHAR(255),
    transaction_date TIMESTAMP NOT NULL,
    amount DECIMAL(18,4) NOT NULL,
    currency VARCHAR(3) NOT NULL,
    payment_method VARCHAR(100) NOT NULL,
    status VARCHAR(50) NOT NULL,
    auth_code VARCHAR(50),
    reference VARCHAR(255),
    brand VARCHAR(100) NOT NULL,
    country VARCHAR(3) NOT NULL,
    merchant_id VARCHAR(100),
    psp VARCHAR(100),
    source_type VARCHAR(50) NOT NULL,
    bronze_record_id UUID NOT NULL,
    source_id VARCHAR(255) NOT NULL,
    run_id UUID NOT NULL,
    rule_pack_version VARCHAR(50) NOT NULL,
    content_hash VARCHAR(128) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

CREATE INDEX idx_silver_transactions_txn_run ON silver_transactions(transaction_id, run_id);
CREATE INDEX idx_silver_transactions_ext_id ON silver_transactions(external_transaction_id);
CREATE INDEX idx_silver_transactions_date ON silver_transactions(transaction_date);

-- Gold Layer Tables
CREATE TABLE gold_reconciliations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    internal_transaction_id UUID NOT NULL,
    psp_transaction_id UUID,
    bank_transaction_id UUID,
    match_status INT NOT NULL,
    match_rule_applied VARCHAR(255),
    match_score DECIMAL(5,2),
    amount_variance DECIMAL(18,4),
    date_variance_days INT,
    is_manual_match BOOLEAN NOT NULL DEFAULT false,
    matched_by VARCHAR(255),
    matched_at TIMESTAMP,
    notes TEXT,
    source_id VARCHAR(255) NOT NULL,
    run_id UUID NOT NULL,
    rule_pack_version VARCHAR(50) NOT NULL,
    content_hash VARCHAR(128) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

CREATE INDEX idx_gold_recon_run_status ON gold_reconciliations(run_id, match_status);

-- Reconciliation Runs
CREATE TABLE reconciliation_runs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    run_name VARCHAR(255) NOT NULL,
    status INT NOT NULL,
    started_at TIMESTAMP NOT NULL,
    completed_at TIMESTAMP,
    rule_pack_version VARCHAR(50) NOT NULL,
    total_records INT NOT NULL DEFAULT 0,
    matched_records INT NOT NULL DEFAULT 0,
    unmatched_records INT NOT NULL DEFAULT 0,
    partial_match_records INT NOT NULL DEFAULT 0,
    match_percentage DECIMAL(5,2) NOT NULL DEFAULT 0,
    error_message TEXT,
    evidence_pack_path VARCHAR(1000),
    evidence_pack_hash VARCHAR(128),
    is_replayable BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

CREATE TABLE run_metrics (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    run_id UUID NOT NULL REFERENCES reconciliation_runs(id),
    metric_name VARCHAR(255) NOT NULL,
    metric_value VARCHAR(500) NOT NULL,
    metric_category VARCHAR(100),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

-- Matching Rules
CREATE TABLE matching_rules (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    rule_name VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    rule_type INT NOT NULL,
    rule_definition TEXT NOT NULL,
    priority INT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    stop_at_first_match BOOLEAN NOT NULL DEFAULT false,
    tolerance_amount DECIMAL(18,4),
    tolerance_window_days INT,
    minimum_score DECIMAL(5,2),
    version VARCHAR(50) NOT NULL,
    effective_from TIMESTAMP NOT NULL,
    effective_to TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

-- Cases
CREATE TABLE cases (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    case_number VARCHAR(50) NOT NULL UNIQUE,
    title VARCHAR(500) NOT NULL,
    description TEXT NOT NULL,
    status INT NOT NULL,
    severity INT NOT NULL,
    variance_type INT NOT NULL,
    assigned_to VARCHAR(255),
    due_date TIMESTAMP,
    resolved_at TIMESTAMP,
    resolved_by VARCHAR(255),
    resolution_notes TEXT,
    linked_transaction_id UUID,
    variance_amount DECIMAL(18,4),
    root_cause_code VARCHAR(100),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

CREATE INDEX idx_cases_status_assigned ON cases(status, assigned_to);

CREATE TABLE case_comments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    case_id UUID NOT NULL REFERENCES cases(id),
    comment TEXT NOT NULL,
    commented_by VARCHAR(255) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

CREATE TABLE case_attachments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    case_id UUID NOT NULL REFERENCES cases(id),
    file_name VARCHAR(500) NOT NULL,
    file_path VARCHAR(1000) NOT NULL,
    file_size_bytes BIGINT NOT NULL,
    content_type VARCHAR(255) NOT NULL,
    uploaded_by VARCHAR(255) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

CREATE TABLE case_labels (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    case_id UUID NOT NULL REFERENCES cases(id),
    label_name VARCHAR(100) NOT NULL,
    label_color VARCHAR(7),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

-- Configuration Tables
CREATE TABLE psp_profiles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    psp_name VARCHAR(255) NOT NULL,
    psp_code VARCHAR(50) NOT NULL UNIQUE,
    file_format VARCHAR(50) NOT NULL,
    time_zone VARCHAR(50) NOT NULL DEFAULT 'UTC',
    supported_currencies VARCHAR(500) NOT NULL,
    merchant_ids TEXT,
    settlement_schedule TEXT,
    is_active BOOLEAN NOT NULL DEFAULT true,
    configuration_json TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

CREATE TABLE field_mappings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    psp_profile_id UUID NOT NULL REFERENCES psp_profiles(id),
    source_field VARCHAR(255) NOT NULL,
    target_field VARCHAR(255) NOT NULL,
    transform_expression TEXT,
    version VARCHAR(50) NOT NULL,
    effective_from TIMESTAMP NOT NULL,
    effective_to TIMESTAMP,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

CREATE TABLE status_mappings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    psp_profile_id UUID NOT NULL REFERENCES psp_profiles(id),
    psp_status VARCHAR(100) NOT NULL,
    canonical_status VARCHAR(100) NOT NULL,
    description TEXT,
    version VARCHAR(50) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

-- Finance Tables
CREATE TABLE fee_contracts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    psp_profile_id UUID NOT NULL REFERENCES psp_profiles(id),
    contract_name VARCHAR(255) NOT NULL,
    payment_method VARCHAR(100) NOT NULL,
    brand VARCHAR(100) NOT NULL,
    currency VARCHAR(3) NOT NULL,
    fee_structure VARCHAR(50) NOT NULL,
    fixed_fee DECIMAL(18,4),
    percentage_fee DECIMAL(5,4),
    minimum_fee DECIMAL(18,4),
    maximum_fee DECIMAL(18,4),
    tiered_structure TEXT,
    effective_from TIMESTAMP NOT NULL,
    effective_to TIMESTAMP,
    version VARCHAR(50) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Draft',
    approved_by VARCHAR(255),
    approved_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

CREATE TABLE settlements (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    psp_name VARCHAR(255) NOT NULL,
    merchant_id VARCHAR(100) NOT NULL,
    settlement_date DATE NOT NULL,
    currency VARCHAR(3) NOT NULL,
    gross_amount DECIMAL(18,4) NOT NULL,
    refunds_amount DECIMAL(18,4) NOT NULL DEFAULT 0,
    chargebacks_amount DECIMAL(18,4) NOT NULL DEFAULT 0,
    fees_amount DECIMAL(18,4) NOT NULL DEFAULT 0,
    reserves_amount DECIMAL(18,4) NOT NULL DEFAULT 0,
    fx_adjustment_amount DECIMAL(18,4) NOT NULL DEFAULT 0,
    net_expected_amount DECIMAL(18,4) NOT NULL,
    actual_amount DECIMAL(18,4),
    variance_amount DECIMAL(18,4),
    bank_transaction_id UUID,
    is_reconciled BOOLEAN NOT NULL DEFAULT false,
    reconciled_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

-- Auth Tables
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(255) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(500),
    is_active BOOLEAN NOT NULL DEFAULT true,
    two_factor_enabled BOOLEAN NOT NULL DEFAULT false,
    sso_provider VARCHAR(100),
    sso_user_id VARCHAR(255),
    last_login_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

CREATE INDEX idx_users_email ON users(email);

CREATE TABLE user_roles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id),
    role_name VARCHAR(100) NOT NULL,
    brand VARCHAR(100),
    country VARCHAR(3),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

CREATE TABLE user_permissions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id),
    permission_name VARCHAR(255) NOT NULL,
    resource VARCHAR(255) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

-- Audit Tables
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    action VARCHAR(255) NOT NULL,
    entity_type VARCHAR(255) NOT NULL,
    entity_id UUID,
    old_values TEXT,
    new_values TEXT,
    performed_by VARCHAR(255) NOT NULL,
    ip_address VARCHAR(50),
    user_agent VARCHAR(500),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    created_by VARCHAR(255) NOT NULL,
    updated_by VARCHAR(255)
);

CREATE INDEX idx_audit_logs_entity ON audit_logs(entity_type, entity_id);
CREATE INDEX idx_audit_logs_performed_by ON audit_logs(performed_by);
CREATE INDEX idx_audit_logs_created_at ON audit_logs(created_at);
