using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WorkflowModel = PyClickerRecorder.Workflow.Workflow;

namespace PyClickerRecorder.Workflow
{
    public class WorkflowStorage
    {
        private static readonly string WorkflowsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "PyClickerRecorder", 
            "Workflows");

        private static readonly string WorkflowsFilePath = Path.Combine(WorkflowsDirectory, "workflows.json");

        public async Task<List<WorkflowModel>> LoadWorkflowsAsync()
        {
            try
            {
                if (!File.Exists(WorkflowsFilePath))
                {
                    return new List<WorkflowModel>();
                }

                string json = await File.ReadAllTextAsync(WorkflowsFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                var workflows = JsonSerializer.Deserialize<List<WorkflowModel>>(json, options);
                return workflows ?? new List<WorkflowModel>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load workflows: {ex.Message}", ex);
            }
        }

        public async Task SaveWorkflowsAsync(List<WorkflowModel> workflows)
        {
            try
            {
                Directory.CreateDirectory(WorkflowsDirectory);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(workflows, options);
                await File.WriteAllTextAsync(WorkflowsFilePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save workflows: {ex.Message}", ex);
            }
        }

        public async Task<WorkflowModel> LoadWorkflowAsync(string workflowId)
        {
            var workflows = await LoadWorkflowsAsync();
            return workflows.Find(w => w.Id == workflowId);
        }

        public async Task SaveWorkflowAsync(WorkflowModel workflow)
        {
            var workflows = await LoadWorkflowsAsync();
            var existingIndex = workflows.FindIndex(w => w.Id == workflow.Id);
            
            workflow.ModifiedDate = DateTime.Now;
            
            if (existingIndex >= 0)
            {
                workflows[existingIndex] = workflow;
            }
            else
            {
                workflows.Add(workflow);
            }

            await SaveWorkflowsAsync(workflows);
        }

        public async Task DeleteWorkflowAsync(string workflowId)
        {
            var workflows = await LoadWorkflowsAsync();
            workflows.RemoveAll(w => w.Id == workflowId);
            await SaveWorkflowsAsync(workflows);
        }

        public async Task ExportWorkflowAsync(string workflowId, string filePath)
        {
            var workflow = await LoadWorkflowAsync(workflowId);
            if (workflow == null)
            {
                throw new ArgumentException($"Workflow with ID '{workflowId}' not found.");
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(workflow, options);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<WorkflowModel> ImportWorkflowAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Workflow file not found: {filePath}");
            }

            try
            {
                string json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var workflow = JsonSerializer.Deserialize<WorkflowModel>(json, options);
                if (workflow == null)
                {
                    throw new InvalidOperationException("Failed to deserialize workflow from file.");
                }

                // Generate new ID to avoid conflicts
                workflow.Id = Guid.NewGuid().ToString();
                workflow.CreatedDate = DateTime.Now;
                workflow.ModifiedDate = DateTime.Now;

                await SaveWorkflowAsync(workflow);
                return workflow;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Invalid workflow file format: {ex.Message}", ex);
            }
        }

        public async Task<bool> WorkflowExistsAsync(string workflowId)
        {
            var workflows = await LoadWorkflowsAsync();
            return workflows.Exists(w => w.Id == workflowId);
        }

        public async Task BackupWorkflowsAsync()
        {
            try
            {
                var backupPath = Path.Combine(WorkflowsDirectory, "Backups");
                Directory.CreateDirectory(backupPath);

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFilePath = Path.Combine(backupPath, $"workflows_backup_{timestamp}.json");

                if (File.Exists(WorkflowsFilePath))
                {
                    File.Copy(WorkflowsFilePath, backupFilePath);
                }

                // Keep only the last 10 backups
                var backupFiles = Directory.GetFiles(backupPath, "workflows_backup_*.json");
                if (backupFiles.Length > 10)
                {
                    Array.Sort(backupFiles);
                    for (int i = 0; i < backupFiles.Length - 10; i++)
                    {
                        File.Delete(backupFiles[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw - backup failure shouldn't stop the application
                Console.WriteLine($"Backup failed: {ex.Message}");
            }
        }
    }
}