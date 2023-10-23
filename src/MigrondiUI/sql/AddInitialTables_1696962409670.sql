-- MIGRONDI:NAME=AddInitialTables_1696962409670.sql
-- MIGRONDI:TIMESTAMP=1696962409670
-- ---------- MIGRONDI:UP ----------
-- Add your SQL migration code below. You can delete this line but do not delete the comments above.
create table workspaces(
    Id integer primary key autoincrement,
    Name text not null,
    Path text not null
);

create table projects(
    Id integer primary key autoincrement,
    Name text not null,
    Path text not null,
    WorkspaceId integer not null,
    foreign key(WorkspaceId) references workspaces(Id) on delete cascade on update cascade
);

create table migrondi_configs(
    id integer primary key autoincrement,
    connection text not null,
    migrations text not null,
    driver text not null,
    projectId integer not null,
    foreign key(projectId) references projects(Id) on delete cascade on update cascade
);

create table migrondi_migrations(
    id integer primary key autoincrement,
    name text not null,
    timestamp integer not null,
    upContent text not null,
    downContent text not null,
    projectId integer not null,
    configId integer not null,
    foreign key(configId) references migrondi_configs(id) on delete cascade on update cascade,
    foreign key(projectId) references projects(Id) on delete cascade on update cascade
);

-- ---------- MIGRONDI:DOWN ----------
-- Add your SQL rollback code below. You can delete this line but do not delete the comment above.


