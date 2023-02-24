// SPDX-License-Identifier: MIT
pragma solidity >=0.4.25 <0.9.0;

contract SignaturesContract {
    
    uint64 public lastSignatureId = 0;
    uint64 public lastDocumentId = 0;

    struct Document {
        uint64 id;
        uint256 hashCode;
        uint timestamp;
    }
    struct Signature {
        uint64 id;
        uint256 hashCode;
        uint timestamp;
    }

    mapping(uint64 => Document) public documents;
    mapping(uint64 => Signature) public signatures;

    constructor() {
        
    }

    event DocumentCreated(uint64 _id);
    event DocumentUpdated(uint64 _id, uint _timestamp);
    event SignatureCreated(uint64 _id, uint _timestamp);

    function createDocument() public returns (uint64 id) {
        lastDocumentId++;
        documents[lastDocumentId] = Document(lastDocumentId, 0, 0);
        emit DocumentCreated(lastDocumentId);
        return lastDocumentId;
    }
    function updateDocument(uint64 _id, uint256 _hashCode) public returns (uint timestamp) {
        uint time = block.timestamp;
        if(documents[_id].timestamp == 0){
            documents[_id].hashCode = _hashCode;
            documents[_id].timestamp = time;
            emit DocumentUpdated(_id, time);
            return time;
        }
        return 0;
    }
    function requestDocument(uint64 _id) public view returns (uint256 hashCode, uint timestamp) {
        return (documents[_id].hashCode, documents[_id].timestamp);
    }

    function createSignature(uint256 _hashCode) public {
        lastSignatureId++;
        signatures[lastSignatureId] = Signature(lastSignatureId, _hashCode, block.timestamp);
        emit SignatureCreated(lastSignatureId, block.timestamp);
    }
    function requestSignature(uint64 _id) public view returns (uint256 hashCode, uint timestamp) {
        return (signatures[_id].hashCode, signatures[_id].timestamp);
    }

}