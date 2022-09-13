import React, { Component } from "react";
import axios from "axios";

export class Home extends Component {
  static displayName = Home.name;

  state = {
    selectedFile: null,
  };

  onFileChange = (event) => {
    this.setState({ selectedFile: event.target.files[0] });
    this.setState({ result: undefined });
  };

  onResult = (response) => {
    this.setState({ result: response.data });
  };

  onFileUpload = () => {
    const formData = new FormData();

    formData.append(
      "file",
      this.state.selectedFile,
      this.state.selectedFile.name
    );

    axios.post("meter-reading-uploads", formData).then(this.onResult);
  };

  fileData = () => {
    if (this.state.selectedFile) {
      return (
        <div>
          <h2>File Details:</h2>
          <p>File Name: {this.state.selectedFile.name}</p>
          <p>File Type: {this.state.selectedFile.type}</p>
          <p>
            Last Modified:{" "}
            {this.state.selectedFile.lastModifiedDate.toDateString()}
          </p>
          <div>
            {!this.state.result && (
              <button className="btn btn-primary" onClick={this.onFileUpload}>
                Upload
              </button>
            )}
          </div>
        </div>
      );
    }
  };

  resultData = () => {
    if (this.state.result) {
      return (
        <div>
          <h2>Result</h2>
          <p>Successful: {this.state.result.successful}</p>
          <p>Failed: {this.state.result.failed}</p>
        </div>
      );
    }
  };

  render() {
    return (
      <div>
        <h1>Customer Meter Readings Upload</h1>
        <div className="mb-3">
          <label className="form-label">Select meter readings CSV</label>
          <input
            className="form-control"
            type="file"
            onChange={this.onFileChange}
            accept="text/csv"
          />
        </div>
        {this.fileData()}
        {this.resultData()}
      </div>
    );
  }
}
