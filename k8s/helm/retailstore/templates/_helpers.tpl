{{/*
Common labels applied to every resource.
kubernetes.io/managed-by lets tooling know Helm owns these resources.
*/}}
{{- define "retailstore.labels" -}}
app.kubernetes.io/managed-by: {{ .Release.Service }}
helm.sh/chart: {{ .Chart.Name }}-{{ .Chart.Version }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
